using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MunicipalElections.Web.Data;
using MunicipalElections.Web.Models;

namespace MunicipalElections.Web.Controllers;

public class HomeController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var municipalities = await context.Municipalities
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .Select(m => new HomeMunicipalityViewModel
            {
                MunicipalityId = m.MunicipalityId,
                Name = m.Name,
                Province = m.Province,
                ElectionDate = m.ElectionDate,
                Description = m.Description,
                LogoFileName = m.LogoFileName,
                CandidateCount = m.Positions.SelectMany(p => p.Candidates).Count()
            })
            .ToListAsync();

        return View(new HomeViewModel { Municipalities = municipalities });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
