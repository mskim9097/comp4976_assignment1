using MEMS.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace MEMS.Data.Services;

// Counts go through the tenant filter: a restricted caller only counts their own municipality.
public class DashboardService(MemsDbContext db)
{
    public async Task<DashboardCounts> GetCountsAsync(CancellationToken ct = default) => new(
        await db.Municipalities.CountAsync(ct),
        await db.Positions.CountAsync(ct),
        await db.Candidates.CountAsync(ct));
}
