using System.Security.Claims;
using MEMS.Data.Entities;
using MEMS.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MEMS.Data;

// Adds the tenant of a Municipality Admin to the login cookie of the Admin app.
public class TenantClaimsPrincipalFactory(
    UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles, IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(users, roles, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        if (user.MunicipalityId is int tenantId)
        {
            identity.AddClaim(new Claim(ClaimNames.MunicipalityId, tenantId.ToString()));
        }
        return identity;
    }
}
