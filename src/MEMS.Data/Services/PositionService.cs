using MEMS.Data.Entities;
using MEMS.Shared;
using MEMS.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace MEMS.Data.Services;

// Positions of one municipality. Every query goes through the tenant filter of MemsDbContext, so a position of
// another municipality is simply "not found" for a restricted caller.
public class PositionService(MemsDbContext db, ImageFileStore images)
{
    public Task<List<Position>> ListAsync(int municipalityId, CancellationToken ct = default) =>
        db.Positions.AsNoTracking().Include(p => p.Municipality)
            .Where(p => p.MunicipalityId == municipalityId)
            .OrderBy(p => p.PositionId)
            .ToListAsync(ct);

    // Every position the caller may see (all municipalities for a Super Admin), with the number of candidates.
    public async Task<List<PositionListItem>> ListAllAsync(CancellationToken ct = default) =>
        await db.Positions.AsNoTracking().Include(p => p.Municipality)
            .OrderBy(p => p.Municipality!.Name).ThenBy(p => p.PositionId)
            .Select(p => new PositionListItem(p, p.Candidates.Count))
            .ToListAsync(ct);

    public async Task<Position> GetAsync(int id, CancellationToken ct = default) =>
        await db.Positions.AsNoTracking().Include(p => p.Municipality).FirstOrDefaultAsync(p => p.PositionId == id, ct)
        ?? throw new NotFoundException("Position not found.");

    public async Task<Position> CreateAsync(int municipalityId, PositionRequest request, CancellationToken ct = default)
    {
        var municipality = await db.Municipalities.FirstOrDefaultAsync(m => m.MunicipalityId == municipalityId, ct)
            ?? throw new NotFoundException("Municipality not found.");
        var name = Provinces.NormalizeName(request.Name);
        await ValidateAsync(name, request.SeatsAvailable, municipalityId, null, ct);

        var position = new Position { Name = name, SeatsAvailable = request.SeatsAvailable, Municipality = municipality };
        db.Positions.Add(position);
        await SaveAsync(ct);
        return position;
    }

    public async Task<Position> UpdateAsync(int id, PositionRequest request, CancellationToken ct = default)
    {
        var position = await db.Positions.Include(p => p.Municipality).FirstOrDefaultAsync(p => p.PositionId == id, ct)
            ?? throw new NotFoundException("Position not found.");
        var name = Provinces.NormalizeName(request.Name);
        await ValidateAsync(name, request.SeatsAvailable, position.MunicipalityId, id, ct);

        position.Name = name;
        position.SeatsAvailable = request.SeatsAvailable;
        await SaveAsync(ct);
        return position;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var position = await db.Positions.FirstOrDefaultAsync(p => p.PositionId == id, ct)
            ?? throw new NotFoundException("Position not found.");

        // Its candidates are removed with it (cascade); their uploaded photos go too.
        var photos = await db.Candidates.Where(c => c.PositionId == id).Select(c => c.PhotoFileName).ToListAsync(ct);
        db.Positions.Remove(position);
        await db.SaveChangesAsync(ct);
        foreach (var photo in photos)
        {
            images.DeleteUploaded(ImageFolder.Candidates, photo);
        }
    }

    private async Task ValidateAsync(string name, int seats, int municipalityId, int? excludeId, CancellationToken ct)
    {
        if (name.Length == 0)
        {
            throw new RuleViolationException("Position name is required.");
        }
        if (seats < Limits.SeatsMin || seats > Limits.SeatsMax)
        {
            throw new RuleViolationException($"Seats must be between {Limits.SeatsMin} and {Limits.SeatsMax}.");
        }

        var lower = name.ToLower();
        if (await db.Positions.AnyAsync(
                p => p.MunicipalityId == municipalityId && (excludeId == null || p.PositionId != excludeId) && p.Name.ToLower() == lower, ct))
        {
            throw new ConflictException($"This municipality already has a position named '{name}'.");
        }
    }

    private async Task SaveAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("This municipality already has a position with that name.");
        }
    }
}

public record PositionListItem(Position Position, int CandidateCount);
