using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MunicipalElections.Web.Data;
using MunicipalElections.Web.Dtos;
using MunicipalElections.Web.Models;

namespace MunicipalElections.Web.Controllers.Api;

[ApiController]
[Route("api/candidates")]
public class CandidatesApiController(ApplicationDbContext context) : ControllerBase
{
    // Optional ?municipalityId= and ?positionId= filters support the app's candidate directory.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CandidateDto>>> GetAll(
        [FromQuery] int? municipalityId, [FromQuery] int? positionId)
    {
        return Ok(await QueryCandidates(null, municipalityId, positionId));
    }

    // GET /api/candidates/search?name=  (case-insensitive "contains"; blank name returns everyone)
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<CandidateDto>>> Search(
        [FromQuery] string? name, [FromQuery] int? municipalityId, [FromQuery] int? positionId)
    {
        return Ok(await QueryCandidates(name, municipalityId, positionId));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CandidateDto>> GetById(int id)
    {
        var candidate = await context.Candidates
            .AsNoTracking()
            .Include(c => c.Position!).ThenInclude(p => p.Municipality)
            .FirstOrDefaultAsync(c => c.CandidateId == id);
        return candidate is null ? NotFound() : Ok(CandidateDto.From(candidate, Request));
    }

    private async Task<List<CandidateDto>> QueryCandidates(string? name, int? municipalityId, int? positionId)
    {
        IQueryable<Candidate> query = context.Candidates
            .AsNoTracking()
            .Include(c => c.Position!).ThenInclude(p => p.Municipality);

        if (municipalityId is not null)
        {
            query = query.Where(c => c.Position!.MunicipalityId == municipalityId);
        }
        if (positionId is not null)
        {
            query = query.Where(c => c.PositionId == positionId);
        }
        if (!string.IsNullOrWhiteSpace(name))
        {
            var escaped = name.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{escaped}%", "\\"));
        }

        var candidates = await query.OrderBy(c => c.Name).ToListAsync();
        return candidates.Select(c => CandidateDto.From(c, Request)).ToList();
    }
}
