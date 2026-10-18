using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MunicipalElections.Web.Data;
using MunicipalElections.Web.Models;

namespace MunicipalElections.Web.Controllers;

// Public page: the elected positions of each municipality, with seats and number of candidates.
public class PositionsController(ApplicationDbContext context) : Controller
{
    // GET: Positions?municipalityId=1
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(int? municipalityId)
    {
        var municipalities = await context.Municipalities
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .ToListAsync();

        var selected = municipalities.Where(m => municipalityId is null || m.MunicipalityId == municipalityId);
        var positions = await context.Positions
            .AsNoTracking()
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
