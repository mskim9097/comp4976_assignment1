using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MunicipalElections.Web.Areas.Admin.Models;
using MunicipalElections.Web.Data;
using MunicipalElections.Web.Services;

namespace MunicipalElections.Web.Areas.Admin.Controllers;

// Landing page of the Admin area: how many municipalities, positions and candidates the administrator manages.
[Area("Admin")]
[Authorize]
public class DashboardController(ApplicationDbContext context, AdminAccess adminAccess) : Controller
{
    public async Task<IActionResult> Index()
    {
        var municipalities = await adminAccess.GetManagedMunicipalitiesAsync(User);
        if (municipalities.Count == 0)
        {
            return Forbid();
        }

        var ids = municipalities.Select(m => m.MunicipalityId).ToList();
        var model = new DashboardViewModel
        {
            IsSuperAdmin = User.IsInRole(DbSeeder.SuperAdminRole),
            MunicipalityCount = municipalities.Count,
            MunicipalityNames = municipalities.Select(m => m.Name).ToList(),
            PositionCount = await context.Positions.CountAsync(p => ids.Contains(p.MunicipalityId)),
            CandidateCount = await context.Candidates.CountAsync(c => ids.Contains(c.Position!.MunicipalityId))
        };
        return View(model);
    }
}
