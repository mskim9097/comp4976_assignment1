using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MunicipalElections.Api.Data;
using MunicipalElections.Api.Dtos;

namespace MunicipalElections.Api.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionsController(ApplicationDbContext context) : ControllerBase
{
    // Optional ?municipalityId= lets the app list the positions of one municipality (filtering).
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PositionDto>>> GetAll([FromQuery] int? municipalityId)
    {
        var query = context.Positions.AsNoTracking().Include(p => p.Municipality).AsQueryable();
        if (municipalityId is not null)
        {
            query = query.Where(p => p.MunicipalityId == municipalityId);
        }

        var positions = await query
            .OrderBy(p => p.Municipality!.Name)
            .ThenBy(p => p.PositionId)
            .ToListAsync();
        return Ok(positions.Select(PositionDto.From));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PositionDto>> GetById(int id)
    {
        var position = await context.Positions
            .AsNoTracking()
            .Include(p => p.Municipality)
            .FirstOrDefaultAsync(p => p.PositionId == id);
        return position is null ? NotFound() : Ok(PositionDto.From(position));
    }
}
