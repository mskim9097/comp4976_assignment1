using System.Security.Claims;
using MEMS.Data;

namespace MEMS.Api.Infrastructure;

// The tenant resolved from the {tenant} route segment of the current request.
public sealed record TenantInfo(int MunicipalityId, string Slug, string Name)
{
    public const string ItemKey = "MEMS.Tenant";
}

public static class PrincipalExtensions
{
    public static TenantScope ToScope(this ClaimsPrincipal user) => TenantScope.FromPrincipal(user);
}
