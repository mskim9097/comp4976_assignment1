using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MunicipalElections.Web.Data;
using MunicipalElections.Web.Models;

namespace MunicipalElections.Web.Controllers;

public class CandidateDirectoryController(ApplicationDbContext context) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(CandidateDirectoryViewModel filters)
    {
        var municipalities = await context.Municipalities
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .ToListAsync();
        var positionsQuery = context.Positions
            .AsNoTracking()
            .Include(p => p.Municipality)
            .AsQueryable();

        if (filters.MunicipalityId is int municipalityId)
        {
            positionsQuery = positionsQuery.Where(p => p.MunicipalityId == municipalityId);
        }

        var positions = await positionsQuery
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
        filters.Positions = positions
            .Select(p => new SelectListItem($"{p.Municipality!.Name} - {p.Name}", p.PositionId.ToString()))
            .ToList();

        return View(filters);
    }
}
