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
}
