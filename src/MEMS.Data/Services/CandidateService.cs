using MEMS.Data.Entities;
using MEMS.Shared;
using MEMS.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace MEMS.Data.Services;

// Candidates of one municipality. The tenant of a candidate is always taken from its position,
// never from the request, and every query goes through the tenant filter of MemsDbContext.
public class CandidateService(MemsDbContext db, ImageFileStore images)
{
    public Task<List<Candidate>> ListAsync(
        int? municipalityId, string? name = null, int? positionId = null, CancellationToken ct = default)
    {
        // municipalityId null = every candidate the caller may see (the tenant filter still applies).
        var query = WithDetails(db.Candidates.AsNoTracking());
        if (municipalityId is not null)
        {
            query = query.Where(c => c.MunicipalityId == municipalityId);
        }
        if (positionId is not null)
        {
            query = query.Where(c => c.PositionId == positionId);
        }
        if (!string.IsNullOrWhiteSpace(name))
        {
            // Search text is escaped so % and _ are searched as normal characters.
            var escaped = name.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{escaped}%", "\\"));
        }
        return query.OrderBy(c => c.Name).ToListAsync(ct);
    }

    public async Task<Candidate> GetAsync(int id, CancellationToken ct = default) =>
        await WithDetails(db.Candidates.AsNoTracking()).FirstOrDefaultAsync(c => c.CandidateId == id, ct)
        ?? throw new NotFoundException("Candidate not found.");

    public async Task<Candidate> CreateAsync(CandidateRequest request, CancellationToken ct = default)
    {
        var position = await FindPositionAsync(request.PositionId, ct);
        var candidate = new Candidate { CreatedDate = DateTime.UtcNow };
        await ApplyAsync(candidate, request, position, null, ct);

        db.Candidates.Add(candidate);
        await db.SaveChangesAsync(ct);
        return candidate;
    }

    public async Task<Candidate> UpdateAsync(int id, CandidateRequest request, CancellationToken ct = default)
    {
        var candidate = await WithDetails(db.Candidates).FirstOrDefaultAsync(c => c.CandidateId == id, ct)
            ?? throw new NotFoundException("Candidate not found.");
        var position = await FindPositionAsync(request.PositionId, ct);
        await ApplyAsync(candidate, request, position, id, ct);

        await db.SaveChangesAsync(ct);
        return candidate;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var candidate = await db.Candidates.FirstOrDefaultAsync(c => c.CandidateId == id, ct)
            ?? throw new NotFoundException("Candidate not found.");
        db.Candidates.Remove(candidate);
        await db.SaveChangesAsync(ct);
        images.DeleteUploaded(ImageFolder.Candidates, candidate.PhotoFileName);
    }

    public async Task<Candidate> SetPhotoAsync(
        int id, string fileName, string contentType, long length, Stream content, CancellationToken ct = default)
    {
        var candidate = await WithDetails(db.Candidates).FirstOrDefaultAsync(c => c.CandidateId == id, ct)
            ?? throw new NotFoundException("Candidate not found.");

        var stored = await images.SaveAsync(ImageFolder.Candidates, fileName, contentType, length, content, ct);
        var previous = candidate.PhotoFileName;
        candidate.PhotoFileName = stored;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            images.DeleteUploaded(ImageFolder.Candidates, stored);
            throw;
        }

        images.DeleteUploaded(ImageFolder.Candidates, previous);
        return candidate;
    }

    private static IQueryable<Candidate> WithDetails(IQueryable<Candidate> query) =>
        query.Include(c => c.Position).Include(c => c.Municipality);

    // Looked up through the tenant filter: a position of another municipality is "not found".
    private async Task<Position> FindPositionAsync(int positionId, CancellationToken ct) =>
        await db.Positions.Include(p => p.Municipality).FirstOrDefaultAsync(p => p.PositionId == positionId, ct)
        ?? throw new NotFoundException("Position not found.");

    private async Task ApplyAsync(Candidate candidate, CandidateRequest request, Position position, int? excludeId, CancellationToken ct)
    {
        var name = Provinces.NormalizeName(request.Name);
        if (name.Length == 0)
        {
            throw new RuleViolationException("Candidate name is required.");
        }

        var website = Clean(request.Website);
        if (website is not null &&
            !(Uri.TryCreate(website, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)))
        {
            throw new RuleViolationException("The website must be an http or https address.");
        }

        var lower = name.ToLower();
        if (await db.Candidates.AnyAsync(
                c => c.PositionId == position.PositionId && (excludeId == null || c.CandidateId != excludeId) && c.Name.ToLower() == lower, ct))
        {
            throw new ConflictException($"A candidate named '{name}' already runs for {position.Name}.");
        }

        candidate.Name = name;
        candidate.Profile = Clean(request.Biography);
        candidate.Website = website;
        candidate.Email = Clean(request.Email);
        candidate.Position = position;
        candidate.PositionId = position.PositionId;
        candidate.MunicipalityId = position.MunicipalityId;
        candidate.Municipality = position.Municipality;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
