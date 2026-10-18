using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MunicipalElections.Web.Data;
using MunicipalElections.Web.Models;

namespace MunicipalElections.Web.Controllers;

public class CandidateDirectoryController(ApplicationDbContext context) : Controller
{
    // GET: CandidateDirectory/Details/5 - one candidate's profile
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var candidate = await context.Candidates
            .AsNoTracking()
            .Include(c => c.Position!)
                .ThenInclude(p => p.Municipality)
            .FirstOrDefaultAsync(c => c.CandidateId == id);
        if (candidate is null)
        {
            return NotFound();
        }

        return View(candidate);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(CandidateDirectoryViewModel filters)
    {
        var municipalities = await context.Municipalities
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .ToListAsync();
        var positions = await context.Positions
            .AsNoTracking()
            .Include(p => p.Municipality)
            .OrderBy(p => p.Municipality!.Name)
            .ThenBy(p => p.Name)
            .ToListAsync();

        IQueryable<Candidate> candidatesQuery = context.Candidates
            .AsNoTracking()
            .Include(c => c.Position!)
                .ThenInclude(p => p.Municipality);

        if (filters.MunicipalityId is int selectedMunicipalityId)
        {
            candidatesQuery = candidatesQuery
                .Where(c => c.Position!.MunicipalityId == selectedMunicipalityId);
        }

        if (filters.PositionId is int positionId)
        {
            candidatesQuery = candidatesQuery.Where(c => c.PositionId == positionId);
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
        // Position list: the chosen municipality's positions, or all positions with their municipality name.
        // (directory-filters.js rebuilds this list in the browser when the Municipality choice changes.)
        filters.Positions = positions
            .Where(p => filters.MunicipalityId is null || p.MunicipalityId == filters.MunicipalityId)
            .Select(p => new SelectListItem(
                filters.MunicipalityId is null ? $"{p.Municipality!.Name} - {p.Name}" : p.Name,
                p.PositionId.ToString()))
            .ToList();
        filters.AllPositions = positions
            .Select(p => new DirectoryPositionOption
            {
                PositionId = p.PositionId,
                Name = p.Name,
                MunicipalityId = p.MunicipalityId,
                MunicipalityName = p.Municipality!.Name
            })
            .ToList();

        return View(filters);
    }
}
