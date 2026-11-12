using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MunicipalElections.Web.Data;
using MunicipalElections.Web.Models;

namespace MunicipalElections.Web.Services;

// Which municipalities a logged-in administrator may manage:
// Super Admin: all of them; municipality admin: the ones whose admin group (role) the user belongs to.
public class AdminAccess(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
{
    public async Task<List<Municipality>> GetManagedMunicipalitiesAsync(ClaimsPrincipal principal)
    {
        if (principal.IsInRole(DbSeeder.SuperAdminRole))
        {
            return await context.Municipalities
                .AsNoTracking()
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return [];
        }

        var roleNames = await userManager.GetRolesAsync(user);
        if (roleNames.Count == 0)
        {
            return [];
        }

        return await context.Municipalities
            .AsNoTracking()
            .Where(m => roleNames.Contains(m.AdminRoleName))
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    // What the public pages may show to the current user. A municipality admin sees only their own municipality
    // (Super Admin: all of them). Administrators do not get the voter's "preferred candidates" tools.
    // Visitors and app users (no admin role) see everything.
    public async Task<PublicScope> GetPublicScopeAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return new PublicScope(false, null);
        }

        if (principal.IsInRole(DbSeeder.SuperAdminRole))
        {
            return new PublicScope(true, null);
        }

        var managed = await GetManagedMunicipalitiesAsync(principal);
        return managed.Count == 0
            ? new PublicScope(false, null)
            : new PublicScope(true, managed.Select(m => m.MunicipalityId).ToList());
    }
}

// MunicipalityIds is null when nothing is restricted.
public record PublicScope(bool IsAdministrator, List<int>? MunicipalityIds)
{
    public bool Allows(int municipalityId) => MunicipalityIds is null || MunicipalityIds.Contains(municipalityId);
}
