using MEMS.Data.Entities;
using MEMS.Shared.Dtos;
using MEMS.Shared.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MEMS.Data.Services;

// Tenant (municipality) management. Reading is public; creating, editing and deleting tenants is Super Admin only.
public class MunicipalityService(MemsDbContext db, ILogger<MunicipalityService> logger)
{
    private sealed record Row(
        int Id, string Name, string Slug, string Province, DateOnly ElectionDate, string? Description,
        string ThemeColor, int LogoVersion, int PositionCount, int CandidateCount);

    private static IQueryable<Row> Project(IQueryable<Municipality> query) =>
        query.Select(m => new Row(
            m.MunicipalityId, m.Name, m.Slug, m.Province, m.ElectionDate, m.Description,
            m.ThemeColor, m.LogoVersion, m.Positions.Count, m.Candidates.Count));

    private static MunicipalityDto ToDto(Row r) => new(
        r.Id, r.Name, r.Slug, r.Province, r.ElectionDate, r.Description, r.ThemeColor,
        r.LogoVersion > 0 ? $"/api/municipalities/{r.Id}/logo?v={r.LogoVersion}" : null,
        r.PositionCount, r.CandidateCount);

    // Anonymous visitors and the Super Admin see every tenant; a Municipality Admin sees only their own.
    public async Task<List<MunicipalityDto>> ListAsync(TenantScope scope, CancellationToken ct = default)
    {
        IQueryable<Municipality> query = db.Municipalities.AsNoTracking();
        if (scope.IsMunicipalityAdmin)
        {
            var own = scope.MunicipalityId ?? -1;
            query = query.Where(m => m.MunicipalityId == own);
        }

        var rows = await Project(query.OrderBy(m => m.Name)).ToListAsync(ct);
        return rows.Select(ToDto).ToList();
    }

    public async Task<MunicipalityDto?> GetAsync(TenantScope scope, int id, CancellationToken ct = default)
    {
        if (!scope.CanSee(id)) return null;
        var row = await Project(db.Municipalities.AsNoTracking().Where(m => m.MunicipalityId == id)).FirstOrDefaultAsync(ct);
        return row is null ? null : ToDto(row);
    }

    public async Task<MunicipalityDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var normalized = SlugRules.Normalize(slug);
        var row = await Project(db.Municipalities.AsNoTracking().Where(m => m.Slug == normalized)).FirstOrDefaultAsync(ct);
        return row is null ? null : ToDto(row);
    }

    public async Task<ServiceResult<MunicipalityDto>> CreateAsync(TenantScope scope, MunicipalityRequest request, CancellationToken ct = default)
    {
        if (!scope.IsSuperAdmin || !scope.IsAuthenticated)
            return ServiceResult<MunicipalityDto>.From(ServiceResult.Forbidden("Only a Super Admin can create municipalities."));

        var errors = await ValidateAsync(request, null, ct);
        if (errors.Count > 0) return ServiceResult<MunicipalityDto>.From(ServiceResult.Invalid(errors));

        var entity = new Municipality();
        Apply(entity, request);
        db.Municipalities.Add(entity);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Could not create municipality {Slug}", request.Slug);
            return ServiceResult<MunicipalityDto>.From(ServiceResult.Conflict(nameof(request.Slug), "A municipality with this name or URL segment already exists."));
        }

        logger.LogInformation("Municipality {MunicipalityId} ({Slug}) created", entity.MunicipalityId, entity.Slug);
        var dto = (await GetAsync(scope, entity.MunicipalityId, ct))!;
        return ServiceResult<MunicipalityDto>.Success(dto, ResultStatus.Created);
    }

    public async Task<ServiceResult<MunicipalityDto>> UpdateAsync(TenantScope scope, int id, MunicipalityRequest request, CancellationToken ct = default)
    {
        if (!scope.IsSuperAdmin || !scope.IsAuthenticated)
            return ServiceResult<MunicipalityDto>.From(ServiceResult.Forbidden("Only a Super Admin can edit municipalities."));

        var entity = await db.Municipalities.FirstOrDefaultAsync(m => m.MunicipalityId == id, ct);
        if (entity is null) return ServiceResult<MunicipalityDto>.From(ServiceResult.NotFound("Municipality not found."));

        var errors = await ValidateAsync(request, id, ct);
        if (errors.Count > 0) return ServiceResult<MunicipalityDto>.From(ServiceResult.Invalid(errors));

        Apply(entity, request);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Could not update municipality {MunicipalityId}", id);
            return ServiceResult<MunicipalityDto>.From(ServiceResult.Conflict(nameof(request.Slug), "A municipality with this name or URL segment already exists."));
        }

        return ServiceResult<MunicipalityDto>.Success((await GetAsync(scope, id, ct))!);
    }

    // Removes the tenant with everything that belongs to it: candidates, positions and its administrator accounts.
    public async Task<ServiceResult> DeleteAsync(TenantScope scope, int id, CancellationToken ct = default)
    {
        if (!scope.IsSuperAdmin || !scope.IsAuthenticated)
            return ServiceResult.Forbidden("Only a Super Admin can delete municipalities.");

        if (!await db.Municipalities.AnyAsync(m => m.MunicipalityId == id, ct))
            return ServiceResult.NotFound("Municipality not found.");

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            await db.Candidates.Where(c => c.MunicipalityId == id).ExecuteDeleteAsync(ct);
            await db.Positions.Where(p => p.MunicipalityId == id).ExecuteDeleteAsync(ct);
            await db.Users.Where(u => u.MunicipalityId == id).ExecuteDeleteAsync(ct);
            await db.Municipalities.Where(m => m.MunicipalityId == id).ExecuteDeleteAsync(ct);
            await tx.CommitAsync(ct);
        });

        logger.LogInformation("Municipality {MunicipalityId} deleted with its positions, candidates and administrators", id);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> SetLogoAsync(TenantScope scope, int id, string fileName, byte[] data, CancellationToken ct = default)
    {
        if (!scope.IsSuperAdmin || !scope.IsAuthenticated)
            return ServiceResult.Forbidden("Only a Super Admin can change a municipality logo.");

        var check = ImageRules.Check(fileName, data);
        if (!check.Ok) return ServiceResult.Invalid("Logo", check.Error!);

        var updated = await db.Municipalities.Where(m => m.MunicipalityId == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.LogoData, data)
                .SetProperty(m => m.LogoContentType, check.ContentType)
                .SetProperty(m => m.LogoVersion, m => m.LogoVersion + 1), ct);
        return updated == 0 ? ServiceResult.NotFound("Municipality not found.") : ServiceResult.Success();
    }

    public async Task<(byte[] Data, string ContentType)?> GetLogoAsync(int id, CancellationToken ct = default)
    {
        var row = await db.Municipalities.AsNoTracking().Where(m => m.MunicipalityId == id && m.LogoData != null)
            .Select(m => new { m.LogoData, m.LogoContentType }).FirstOrDefaultAsync(ct);
        return row is null ? null : (row.LogoData!, row.LogoContentType ?? "application/octet-stream");
    }

    public async Task<DashboardDto> GetDashboardAsync(TenantScope scope, CancellationToken ct = default)
    {
        if (scope.IsSuperAdmin)
        {
            return new DashboardDto(
                await db.Municipalities.CountAsync(ct), await db.Positions.CountAsync(ct), await db.Candidates.CountAsync(ct));
        }

        var id = scope.MunicipalityId ?? -1;
        return new DashboardDto(
            await db.Municipalities.CountAsync(m => m.MunicipalityId == id, ct),
            await db.Positions.ForTenant(id).CountAsync(ct),
            await db.Candidates.ForTenant(id).CountAsync(ct));
    }

    // Server-side validation that DataAnnotations cannot express (uniqueness, reserved slugs, province list).
    private async Task<Dictionary<string, string[]>> ValidateAsync(MunicipalityRequest request, int? currentId, CancellationToken ct)
    {
        var errors = new Dictionary<string, string[]>();

        request.Name = MunicipalityRules.NormalizeName(request.Name);
        request.Slug = SlugRules.Normalize(request.Slug);
        request.Province = request.Province?.Trim() ?? string.Empty;

        if (request.Name.Length < 2 || request.Name.Length > 100)
            errors[nameof(request.Name)] = ["Municipality name must be between 2 and 100 characters."];
        if (!SlugRules.IsValid(request.Slug))
            errors[nameof(request.Slug)] = [SlugRules.IsReserved(request.Slug)
                ? "That URL segment is reserved by the system. Choose another."
                : "Use lowercase letters, numbers and single hyphens only (for example: pitt-meadows)."];
        if (!MunicipalityRules.IsProvince(request.Province))
            errors[nameof(request.Province)] = ["Select a province or territory from the list."];
        if (request.Description?.Length > 2000)
            errors[nameof(request.Description)] = ["The description can be at most 2000 characters."];
        if (System.Text.RegularExpressions.Regex.IsMatch(request.ThemeColor ?? string.Empty, MunicipalityRules.ColorPattern) == false)
            errors[nameof(request.ThemeColor)] = ["Use a color such as #0d6efd."];

        if (!errors.ContainsKey(nameof(request.Name)) &&
            await db.Municipalities.AnyAsync(m => m.MunicipalityId != currentId && m.Name == request.Name, ct))
            errors[nameof(request.Name)] = ["A municipality with this name already exists."];
        if (!errors.ContainsKey(nameof(request.Slug)) &&
            await db.Municipalities.AnyAsync(m => m.MunicipalityId != currentId && m.Slug == request.Slug, ct))
            errors[nameof(request.Slug)] = ["Another municipality already uses this URL segment."];

        return errors;
    }

    private static void Apply(Municipality entity, MunicipalityRequest request)
    {
        entity.Name = request.Name;
        entity.Slug = request.Slug;
        entity.Province = request.Province;
        entity.ElectionDate = request.ElectionDate;
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.ThemeColor = request.ThemeColor.ToLowerInvariant();
    }
}
