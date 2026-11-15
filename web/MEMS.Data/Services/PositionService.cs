using MEMS.Data.Entities;
using MEMS.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MEMS.Data.Services;

// Every method takes the active tenant id; every query is restricted to it with ForTenant().
public class PositionService(MemsDbContext db, ILogger<PositionService> logger)
{
    private static IQueryable<PositionDto> Project(IQueryable<Position> query) =>
        query.Select(p => new PositionDto(p.PositionId, p.Name, p.SeatsAvailable, p.MunicipalityId, p.Candidates.Count));

    public Task<List<PositionDto>> ListAsync(int municipalityId, CancellationToken ct = default) =>
        Project(db.Positions.AsNoTracking().ForTenant(municipalityId).OrderBy(p => p.PositionId)).ToListAsync(ct);

    public Task<PositionDto?> GetAsync(int municipalityId, int id, CancellationToken ct = default) =>
        Project(db.Positions.AsNoTracking().ForTenant(municipalityId).Where(p => p.PositionId == id)).FirstOrDefaultAsync(ct);

    public async Task<ServiceResult<PositionDto>> CreateAsync(TenantScope scope, int municipalityId, PositionRequest request, CancellationToken ct = default)
    {
        if (!scope.CanManage(municipalityId)) return ServiceResult<PositionDto>.From(ServiceResult.Forbidden());

        var name = request.Name?.Trim() ?? string.Empty;
        var invalid = Check(name, request.SeatsAvailable);
        if (invalid is not null) return ServiceResult<PositionDto>.From(invalid);

        if (await db.Positions.ForTenant(municipalityId).AnyAsync(p => p.Name == name, ct))
            return ServiceResult<PositionDto>.From(ServiceResult.Conflict(nameof(request.Name), "A position with this name already exists for the municipality."));

        var entity = new Position { MunicipalityId = municipalityId, Name = name, SeatsAvailable = request.SeatsAvailable };
        db.Positions.Add(entity);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Could not create position {Name} in tenant {MunicipalityId}", name, municipalityId);
            return ServiceResult<PositionDto>.From(ServiceResult.Conflict(nameof(request.Name), "A position with this name already exists for the municipality."));
        }

        return ServiceResult<PositionDto>.Success((await GetAsync(municipalityId, entity.PositionId, ct))!, ResultStatus.Created);
    }

    public async Task<ServiceResult<PositionDto>> UpdateAsync(TenantScope scope, int municipalityId, int id, PositionRequest request, CancellationToken ct = default)
    {
        if (!scope.CanManage(municipalityId)) return ServiceResult<PositionDto>.From(ServiceResult.Forbidden());

        var entity = await db.Positions.ForTenant(municipalityId).FirstOrDefaultAsync(p => p.PositionId == id, ct);
        if (entity is null) return ServiceResult<PositionDto>.From(ServiceResult.NotFound("Position not found."));

        var name = request.Name?.Trim() ?? string.Empty;
        var invalid = Check(name, request.SeatsAvailable);
        if (invalid is not null) return ServiceResult<PositionDto>.From(invalid);

        if (await db.Positions.ForTenant(municipalityId).AnyAsync(p => p.PositionId != id && p.Name == name, ct))
            return ServiceResult<PositionDto>.From(ServiceResult.Conflict(nameof(request.Name), "A position with this name already exists for the municipality."));

        entity.Name = name;
        entity.SeatsAvailable = request.SeatsAvailable;
        await db.SaveChangesAsync(ct);
        return ServiceResult<PositionDto>.Success((await GetAsync(municipalityId, id, ct))!);
    }

    public async Task<ServiceResult> DeleteAsync(TenantScope scope, int municipalityId, int id, CancellationToken ct = default)
    {
        if (!scope.CanManage(municipalityId)) return ServiceResult.Forbidden();

        var entity = await db.Positions.ForTenant(municipalityId).FirstOrDefaultAsync(p => p.PositionId == id, ct);
        if (entity is null) return ServiceResult.NotFound("Position not found.");

        if (await db.Candidates.ForTenant(municipalityId).AnyAsync(c => c.PositionId == id, ct))
            return ServiceResult.Conflict("Position", "A position cannot be deleted while it has candidates.");

        db.Positions.Remove(entity);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    private static ServiceResult? Check(string name, int seats)
    {
        if (name.Length is < 1 or > 100) return ServiceResult.Invalid(nameof(PositionRequest.Name), "Position name is required (100 characters at most).");
        if (seats is < 1 or > 100) return ServiceResult.Invalid(nameof(PositionRequest.SeatsAvailable), "Seats available must be between 1 and 100.");
        return null;
    }
}
