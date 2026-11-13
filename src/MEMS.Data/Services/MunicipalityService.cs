using MEMS.Data.Entities;
using MEMS.Shared;
using MEMS.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace MEMS.Data.Services;

// Municipalities (tenants). Creating, changing and deleting them is for the Super Admin only:
// a scope that is restricted to one tenant is refused here, whatever the caller (Api or Admin) checked before.
public class MunicipalityService(MemsDbContext db, TenantContext tenant, ImageFileStore images)
{
    public Task<List<Municipality>> ListAsync(CancellationToken ct = default) =>
        db.Municipalities.AsNoTracking().OrderBy(m => m.Name).ToListAsync(ct);

    // For the admin list: the municipality plus how many positions, candidates and administrators it has.
    public async Task<List<MunicipalityListItem>> ListWithCountsAsync(CancellationToken ct = default) =>
        await db.Municipalities.AsNoTracking().OrderBy(m => m.Name)
            .Select(m => new MunicipalityListItem(m, m.Positions.Count, m.Candidates.Count, m.Admins.Count))
            .ToListAsync(ct);

    public async Task<Municipality> GetAsync(int id, CancellationToken ct = default) =>
        await db.Municipalities.AsNoTracking().FirstOrDefaultAsync(m => m.MunicipalityId == id, ct)
        ?? throw new NotFoundException("Municipality not found.");

    public async Task<Municipality> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await db.Municipalities.AsNoTracking().FirstOrDefaultAsync(m => m.Slug == slug, ct)
        ?? throw new NotFoundException("Municipality not found.");

    // The one lookup without the tenant filter: finds the tenant named in a URL (/burnaby) or in ?city=Burnaby
    // so the caller can compare it with the tenant of the logged-in administrator. Match by slug or by name.
    public Task<Municipality?> ResolveAsync(string cityOrSlug, CancellationToken ct = default)
    {
        var key = Provinces.NormalizeName(cityOrSlug).ToLower();
        return db.Municipalities.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(m => m.Slug == key || m.Name.ToLower() == key, ct);
    }

    public async Task<Municipality> CreateAsync(MunicipalityRequest request, CancellationToken ct = default)
    {
        EnsureUnrestricted();
        var (name, slug) = await ValidateAsync(request, null, ct);

        var municipality = new Municipality();
        Apply(municipality, request, name, slug);
        db.Municipalities.Add(municipality);
        await SaveAsync(ct);
        return municipality;
    }

    public async Task<Municipality> UpdateAsync(int id, MunicipalityRequest request, CancellationToken ct = default)
    {
        EnsureUnrestricted();
        var municipality = await db.Municipalities.FirstOrDefaultAsync(m => m.MunicipalityId == id, ct)
            ?? throw new NotFoundException("Municipality not found.");
        var (name, slug) = await ValidateAsync(request, id, ct);

        Apply(municipality, request, name, slug);
        await SaveAsync(ct);
        return municipality;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        EnsureUnrestricted();
        var municipality = await db.Municipalities.FirstOrDefaultAsync(m => m.MunicipalityId == id, ct)
            ?? throw new NotFoundException("Municipality not found.");
        if (await db.Users.AnyAsync(u => u.MunicipalityId == id, ct))
        {
            throw new ConflictException("This municipality still has administrator accounts. Remove them first.");
        }

        // Positions and candidates are removed with the municipality (cascade); their photos go too.
        var photos = await db.Candidates.Where(c => c.MunicipalityId == id).Select(c => c.PhotoFileName).ToListAsync(ct);
        db.Municipalities.Remove(municipality);
        await db.SaveChangesAsync(ct);

        images.DeleteUploaded(ImageFolder.Logos, municipality.LogoFileName);
        foreach (var photo in photos)
        {
            images.DeleteUploaded(ImageFolder.Candidates, photo);
        }
    }

    public async Task<Municipality> SetLogoAsync(
        int id, string fileName, string contentType, long length, Stream content, CancellationToken ct = default)
    {
        EnsureUnrestricted();
        var municipality = await db.Municipalities.FirstOrDefaultAsync(m => m.MunicipalityId == id, ct)
            ?? throw new NotFoundException("Municipality not found.");

        var stored = await images.SaveAsync(ImageFolder.Logos, fileName, contentType, length, content, ct);
        var previous = municipality.LogoFileName;
        municipality.LogoFileName = stored;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            images.DeleteUploaded(ImageFolder.Logos, stored);
            throw;
        }

        images.DeleteUploaded(ImageFolder.Logos, previous);
        return municipality;
    }

    private void EnsureUnrestricted()
    {
        if (tenant.IsRestricted)
        {
            throw new ForbiddenException("Only a Super Admin can manage municipalities.");
        }
    }

    private async Task<(string Name, string Slug)> ValidateAsync(MunicipalityRequest request, int? excludeId, CancellationToken ct)
    {
        var name = Provinces.NormalizeName(request.Name);
        var slug = (request.Slug ?? string.Empty).Trim().ToLowerInvariant();

        if (name.Length == 0)
        {
            throw new RuleViolationException("Name is required.");
        }
        if (!Provinces.IsValid(request.Province))
        {
            throw new RuleViolationException("Choose a province from the list.");
        }
        if (!TenantSlug.IsValid(slug))
        {
            throw new RuleViolationException(TenantSlug.ErrorMessage);
        }
        if (TenantSlug.IsReserved(slug))
        {
            throw new RuleViolationException($"'{slug}' is reserved. Choose another URL segment.");
        }
        if (request.ElectionDate is null)
        {
            throw new RuleViolationException("Election date is required.");
        }

        // Names are compared without upper/lower case, so "burnaby" and "Burnaby" cannot both exist.
        var lowerName = name.ToLower();
        if (await db.Municipalities.IgnoreQueryFilters().AnyAsync(
                m => (excludeId == null || m.MunicipalityId != excludeId) && m.Name.ToLower() == lowerName, ct))
        {
            throw new ConflictException($"A municipality named '{name}' already exists.");
        }
        if (await db.Municipalities.IgnoreQueryFilters().AnyAsync(
                m => (excludeId == null || m.MunicipalityId != excludeId) && m.Slug == slug, ct))
        {
            throw new ConflictException($"The URL segment '{slug}' is already used by another municipality.");
        }

        return (name, slug);
    }

    private static void Apply(Municipality municipality, MunicipalityRequest request, string name, string slug)
    {
        municipality.Name = name;
        municipality.Slug = slug;
        municipality.Province = request.Province;
        municipality.ElectionDate = request.ElectionDate!.Value;
        municipality.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        municipality.ThemeColor = request.ThemeColor.ToLowerInvariant();
        municipality.WelcomeMessage = string.IsNullOrWhiteSpace(request.WelcomeMessage) ? null : request.WelcomeMessage.Trim();
    }

    private async Task SaveAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Two requests with the same name or slug at the same time hit the unique indexes.
            throw new ConflictException("A municipality with this name or URL segment already exists.");
        }
    }
}

public record MunicipalityListItem(Municipality Municipality, int PositionCount, int CandidateCount, int AdminCount);
