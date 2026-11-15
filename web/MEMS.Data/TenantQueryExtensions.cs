using MEMS.Data.Entities;

namespace MEMS.Data;

public static class TenantQueryExtensions
{
    // Row-level isolation: every query on Position / Candidate is restricted to one tenant here.
    public static IQueryable<T> ForTenant<T>(this IQueryable<T> query, int municipalityId) where T : class, ITenantOwned =>
        query.Where(e => e.MunicipalityId == municipalityId);
}
