using MEMS.Data.Entities;
using MEMS.Shared.Dtos;
using MEMS.Shared.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MEMS.Data.Services;

public class CandidateService(MemsDbContext db, ILogger<CandidateService> logger)
{
    private sealed record Row(
        int Id, string Name, string? Profile, string? Website, string? Email, int PositionId, string PositionName,
        int MunicipalityId, string Slug, int PhotoVersion, DateTime CreatedUtc);

    // Photos (blobs) are never loaded by list queries: only the version number is read.
    private static IQueryable<Row> Project(IQueryable<Candidate> query) =>
        query.Select(c => new Row(
            c.CandidateId, c.Name, c.Profile, c.Website, c.Email, c.PositionId, c.Position!.Name,
            c.MunicipalityId, c.Municipality!.Slug, c.PhotoVersion, c.CreatedUtc));

    private static CandidateDto ToDto(Row r) => new(
        r.Id, r.Name, r.Profile, r.Website, r.Email, r.PositionId, r.PositionName, r.MunicipalityId,
        r.PhotoVersion > 0 ? $"/api/{r.Slug}/candidates/{r.Id}/photo?v={r.PhotoVersion}" : null,
        DateTime.SpecifyKind(r.CreatedUtc, DateTimeKind.Utc));

    public async Task<List<CandidateDto>> ListAsync(int municipalityId, string? search = null, int? positionId = null, CancellationToken ct = default)
    {
        var query = db.Candidates.AsNoTracking().ForTenant(municipalityId);
        if (positionId is int pid) query = query.Where(c => c.PositionId == pid);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.Name.Contains(term));
        }

        var rows = await Project(query.OrderBy(c => c.Position!.PositionId).ThenBy(c => c.Name)).ToListAsync(ct);
        return rows.Select(ToDto).ToList();
    }

    public async Task<CandidateDto?> GetAsync(int municipalityId, int id, CancellationToken ct = default)
    {
        var row = await Project(db.Candidates.AsNoTracking().ForTenant(municipalityId).Where(c => c.CandidateId == id)).FirstOrDefaultAsync(ct);
        return row is null ? null : ToDto(row);
    }

    public async Task<ServiceResult<CandidateDto>> CreateAsync(TenantScope scope, int municipalityId, CandidateRequest request, CancellationToken ct = default)
    {
        if (!scope.CanManage(municipalityId)) return ServiceResult<CandidateDto>.From(ServiceResult.Forbidden());

        var invalid = await ValidateAsync(municipalityId, request, ct);
        if (invalid is not null) return ServiceResult<CandidateDto>.From(invalid);

        var entity = new Candidate { MunicipalityId = municipalityId, CreatedUtc = DateTime.UtcNow };
        Apply(entity, request);
        db.Candidates.Add(entity);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Candidate {CandidateId} created in tenant {MunicipalityId}", entity.CandidateId, municipalityId);
        return ServiceResult<CandidateDto>.Success((await GetAsync(municipalityId, entity.CandidateId, ct))!, ResultStatus.Created);
    }

    public async Task<ServiceResult<CandidateDto>> UpdateAsync(TenantScope scope, int municipalityId, int id, CandidateRequest request, CancellationToken ct = default)
    {
        if (!scope.CanManage(municipalityId)) return ServiceResult<CandidateDto>.From(ServiceResult.Forbidden());

        var entity = await db.Candidates.ForTenant(municipalityId).FirstOrDefaultAsync(c => c.CandidateId == id, ct);
        if (entity is null) return ServiceResult<CandidateDto>.From(ServiceResult.NotFound("Candidate not found."));

        var invalid = await ValidateAsync(municipalityId, request, ct);
        if (invalid is not null) return ServiceResult<CandidateDto>.From(invalid);

        Apply(entity, request);
        await db.SaveChangesAsync(ct);
        return ServiceResult<CandidateDto>.Success((await GetAsync(municipalityId, id, ct))!);
    }

    public async Task<ServiceResult> DeleteAsync(TenantScope scope, int municipalityId, int id, CancellationToken ct = default)
    {
        if (!scope.CanManage(municipalityId)) return ServiceResult.Forbidden();

        var deleted = await db.Candidates.ForTenant(municipalityId).Where(c => c.CandidateId == id).ExecuteDeleteAsync(ct);
        if (deleted == 0) return ServiceResult.NotFound("Candidate not found.");

        logger.LogInformation("Candidate {CandidateId} deleted from tenant {MunicipalityId}", id, municipalityId);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> SetPhotoAsync(TenantScope scope, int municipalityId, int id, string fileName, byte[] data, CancellationToken ct = default)
    {
        if (!scope.CanManage(municipalityId)) return ServiceResult.Forbidden();

        var check = ImageRules.Check(fileName, data);
        if (!check.Ok) return ServiceResult.Invalid("Photo", check.Error!);

        var updated = await db.Candidates.ForTenant(municipalityId).Where(c => c.CandidateId == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.PhotoData, data)
                .SetProperty(c => c.PhotoContentType, check.ContentType)
                .SetProperty(c => c.PhotoVersion, c => c.PhotoVersion + 1), ct);
        return updated == 0 ? ServiceResult.NotFound("Candidate not found.") : ServiceResult.Success();
    }

    public async Task<(byte[] Data, string ContentType)?> GetPhotoAsync(int municipalityId, int id, CancellationToken ct = default)
    {
        var row = await db.Candidates.AsNoTracking().ForTenant(municipalityId)
            .Where(c => c.CandidateId == id && c.PhotoData != null)
            .Select(c => new { c.PhotoData, c.PhotoContentType }).FirstOrDefaultAsync(ct);
        return row is null ? null : (row.PhotoData!, row.PhotoContentType ?? "application/octet-stream");
    }

    // The position must belong to the SAME tenant: a candidate can never be attached to another municipality's position.
    private async Task<ServiceResult?> ValidateAsync(int municipalityId, CandidateRequest request, CancellationToken ct)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 100)
            errors[nameof(request.Name)] = ["Candidate name is required (100 characters at most)."];
        if (request.Profile?.Length > 2000)
            errors[nameof(request.Profile)] = ["The biography / profile can be at most 2000 characters."];
        if (!string.IsNullOrWhiteSpace(request.Website) &&
            !(Uri.TryCreate(request.Website.Trim(), UriKind.Absolute, out var uri) && (uri.Scheme is "http" or "https")))
            errors[nameof(request.Website)] = ["Enter a valid web address starting with http:// or https://."];
        if (!string.IsNullOrWhiteSpace(request.Email) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(request.Email.Trim()))
            errors[nameof(request.Email)] = ["Enter a valid email address."];
        if (!await db.Positions.ForTenant(municipalityId).AnyAsync(p => p.PositionId == request.PositionId, ct))
            errors[nameof(request.PositionId)] = ["Select a position that belongs to this municipality."];

        return errors.Count == 0 ? null : ServiceResult.Invalid(errors);
    }

    private static void Apply(Candidate entity, CandidateRequest request)
    {
        entity.Name = request.Name.Trim();
        entity.PositionId = request.PositionId;
        entity.Profile = string.IsNullOrWhiteSpace(request.Profile) ? null : request.Profile.Trim();
        entity.Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim();
        entity.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
    }
}
