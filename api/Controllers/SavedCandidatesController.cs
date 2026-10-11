using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using MunicipalElections.Api.Data;
using MunicipalElections.Api.Dtos;
using MunicipalElections.Api.Models;

namespace MunicipalElections.Api.Controllers;

// The logged-in user's personal voting guide. Every query is scoped to the user id in the JWT.
[Authorize]
[ApiController]
[Route("api/savedcandidates")]
public class SavedCandidatesController(ApplicationDbContext context) : ControllerBase
{
    private string UserId => User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SavedCandidateDto>>> GetAll()
    {
        var saved = await context.SavedCandidates
            .AsNoTracking()
            .Where(s => s.UserId == UserId)
            .Include(s => s.Candidate!).ThenInclude(c => c.Position)
            .Include(s => s.Candidate!).ThenInclude(c => c.Municipality)
            .OrderByDescending(s => s.DateSaved)
            .ThenByDescending(s => s.SavedCandidateId)
            .ToListAsync();
        return Ok(saved.Select(s => SavedCandidateDto.From(s, Request)));
    }

    [HttpPost]
    public async Task<ActionResult<SavedCandidateDto>> Add(SaveCandidateRequest request)
    {
        var candidate = await context.Candidates
            .AsNoTracking()
            .Include(c => c.Position)
            .Include(c => c.Municipality)
            .FirstOrDefaultAsync(c => c.CandidateId == request.CandidateId);
        if (candidate is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Candidate not found.");
        }

        if (await context.SavedCandidates.AnyAsync(s => s.UserId == UserId && s.CandidateId == candidate.CandidateId))
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Candidate is already in your voting guide.");
        }

        // Whole seconds only, so the iOS ISO-8601 decoder can read the value.
        var now = DateTime.UtcNow;
        var saved = new SavedCandidate
        {
            UserId = UserId,
            CandidateId = candidate.CandidateId,
            DateSaved = new DateTime(now.Ticks - now.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc)
        };
        context.SavedCandidates.Add(saved);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Two simultaneous requests for the same candidate hit the unique index.
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Candidate is already in your voting guide.");
        }

        saved.Candidate = candidate;
        return StatusCode(StatusCodes.Status201Created, SavedCandidateDto.From(saved, Request));
    }

    // {id} is the SavedCandidateId. Someone else's entry looks the same as a missing one (404).
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remove(int id)
    {
        var saved = await context.SavedCandidates
            .FirstOrDefaultAsync(s => s.SavedCandidateId == id && s.UserId == UserId);
        if (saved is null)
        {
            return NotFound();
        }

        context.SavedCandidates.Remove(saved);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
