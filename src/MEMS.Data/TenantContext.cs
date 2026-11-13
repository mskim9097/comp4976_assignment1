namespace MEMS.Data;

// The tenant of the current request or Blazor circuit. Once it is restricted, every Municipality, Position and
// Candidate query is filtered to that municipality (see MemsDbContext). Not restricted = all tenants
// (Super Admin, start-up seeding and the public list of municipalities).
public class TenantContext
{
    public int? MunicipalityId { get; private set; }

    public bool IsRestricted => MunicipalityId.HasValue;

    // Can be called again with the same id, never with another one: a scope cannot jump to a second tenant.
    public void Restrict(int municipalityId)
    {
        if (MunicipalityId is int current && current != municipalityId)
        {
            throw new InvalidOperationException("This scope is already restricted to another tenant.");
        }

        MunicipalityId = municipalityId;
    }
}
