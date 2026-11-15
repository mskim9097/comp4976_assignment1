using System.Security.Claims;
using MEMS.Data.Entities;
using MEMS.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MEMS.Data;

// Adds the user's tenant to the sign-in cookie so every request knows which municipality the administrator belongs to.
public class MemsClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        if (user.MunicipalityId is int tenantId)
        {
            identity.AddClaim(new Claim(MemsClaims.MunicipalityId, tenantId.ToString()));
        }
        if (!string.IsNullOrWhiteSpace(user.DisplayName))
        {
            identity.AddClaim(new Claim("display_name", user.DisplayName));
        }
        return identity;
    }
}
