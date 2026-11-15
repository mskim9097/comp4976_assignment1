using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MunicipalElections.Web.Data;
using MunicipalElections.Web.Models;

namespace MunicipalElections.Web.Controllers;

public class CandidateDirectoryController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(CandidateDirectoryViewModel filters)
    {
        filters ??= new CandidateDirectoryViewModel();

        var userRoleNames = await GetCurrentUserRoleNamesAsync();
        var managedMunicipalityIds = await GetManagedMunicipalityIdsAsync(userRoleNames);
        var isAdmin = userRoleNames.Contains(DbSeeder.SuperAdminRole) || managedMunicipalityIds.Count > 0;
        filters.CanShortlistCandidates = !isAdmin;

        var municipalitiesQuery = context.Municipalities
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .AsQueryable();

        if (managedMunicipalityIds.Count > 0)
        {
            municipalitiesQuery = municipalitiesQuery.Where(m => managedMunicipalityIds.Contains(m.MunicipalityId));
        }

        var municipalities = await municipalitiesQuery.ToListAsync();

        var positionsQuery = context.Positions
            .AsNoTracking()
            .Include(p => p.Municipality)
            .AsQueryable();

        if (managedMunicipalityIds.Count > 0)
        {
            positionsQuery = positionsQuery.Where(p => managedMunicipalityIds.Contains(p.MunicipalityId));
        }

        if (filters.MunicipalityId is int municipalityId)
        {
            if (managedMunicipalityIds.Count > 0 && !managedMunicipalityIds.Contains(municipalityId))
            {
                filters.MunicipalityId = null;
            }
            else
            {
                positionsQuery = positionsQuery.Where(p => p.MunicipalityId == municipalityId);
            }
        }

        var positions = await positionsQuery
            .OrderBy(p => p.Municipality!.Name)
            .ThenBy(p => p.Name)
            .ToListAsync();

        IQueryable<Candidate> candidatesQuery = context.Candidates
            .AsNoTracking()
            .Include(c => c.Position!)
                .ThenInclude(p => p.Municipality);

        if (managedMunicipalityIds.Count > 0)
        {
            candidatesQuery = candidatesQuery.Where(c => managedMunicipalityIds.Contains(c.Position!.MunicipalityId));
        }

        if (filters.MunicipalityId is int selectedMunicipalityId)
        {
            candidatesQuery = candidatesQuery
                .Where(c => c.Position!.MunicipalityId == selectedMunicipalityId);
        }

        if (filters.PositionId is int positionId)
        {
            if (managedMunicipalityIds.Count > 0 && !positions.Any(p => p.PositionId == positionId))
            {
                filters.PositionId = null;
            }
            else
            {
                candidatesQuery = candidatesQuery.Where(c => c.PositionId == positionId);
            }
        }

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var escaped = filters.Search.Trim()
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
            var pattern = $"%{escaped}%";
            candidatesQuery = candidatesQuery.Where(c =>
                EF.Functions.Like(c.Name, pattern, "\\") ||
                (c.Profile != null && EF.Functions.Like(c.Profile, pattern, "\\")) ||
                (c.Email != null && EF.Functions.Like(c.Email, pattern, "\\")));
        }

        filters.Candidates = await candidatesQuery
            .OrderBy(c => c.Name)
            .ToListAsync();
        filters.Municipalities = municipalities
            .Select(m => new SelectListItem(m.Name, m.MunicipalityId.ToString()))
            .ToList();
        filters.MunicipalityElectionDates = municipalities.ToDictionary(
            m => m.Name,
            m => m.ElectionDate.ToString("yyyy-MM-dd"));
        filters.Positions = positions
            .Select(p => new SelectListItem($"{p.Municipality!.Name} - {p.Name}", p.PositionId.ToString()))
            .ToList();

        return View(filters);
    }

    private async Task<HashSet<string>> GetCurrentUserRoleNamesAsync()
    {
        if (User.Identity is null || !User.Identity.IsAuthenticated)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        var roles = await userManager.GetRolesAsync(user);
        return new HashSet<string>(roles, StringComparer.Ordinal);
    }

    private async Task<List<int>> GetManagedMunicipalityIdsAsync(ISet<string> userRoles)
    {
        if (userRoles.Count == 0)
        {
            return [];
        }

        if (userRoles.Contains(DbSeeder.SuperAdminRole))
        {
            return [];
        }

        var adminRoleNames = await context.Municipalities
            .AsNoTracking()
            .Where(m => userRoles.Contains(m.AdminRoleName))
            .Select(m => m.MunicipalityId)
            .ToListAsync();

        return adminRoleNames;
    }
}
