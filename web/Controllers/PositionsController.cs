using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MunicipalElections.Web.Data;
using MunicipalElections.Web.Models;
using MunicipalElections.Web.Services;

namespace MunicipalElections.Web.Controllers;

// Public page: the elected positions of each municipality, with seats and number of candidates.
public class PositionsController(ApplicationDbContext context, AdminAccess adminAccess) : Controller
{
    // GET: Positions?municipalityId=1
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(int? municipalityId)
    {
        var scope = await adminAccess.GetPublicScopeAsync(User);
        var municipalitiesQuery = context.Municipalities.AsNoTracking();
        var positionsQuery = context.Positions.AsNoTracking();
        if (scope.MunicipalityIds is { } ids)
        {
            municipalitiesQuery = municipalitiesQuery.Where(m => ids.Contains(m.MunicipalityId));
            positionsQuery = positionsQuery.Where(p => ids.Contains(p.MunicipalityId));
        }

        var municipalities = await municipalitiesQuery
            .OrderBy(m => m.Name)
            .ToListAsync();

        var selected = municipalities.Where(m => municipalityId is null || m.MunicipalityId == municipalityId);
        var positions = await positionsQuery
            .OrderBy(p => p.PositionId)
            .Select(p => new PositionBrowseItem { Position = p, CandidateCount = p.Candidates.Count })
            .ToListAsync();

        var model = new PositionBrowseViewModel
        {
            MunicipalityId = municipalityId,
            Municipalities = municipalities
                .Select(m => new SelectListItem(m.Name, m.MunicipalityId.ToString()))
                .ToList(),
            Groups = selected
                .Select(m => new PositionBrowseMunicipality
                {
                    Municipality = m,
                    Positions = positions.Where(p => p.Position.MunicipalityId == m.MunicipalityId).ToList()
                })
                .ToList()
        };
        return View(model);
    }
}
