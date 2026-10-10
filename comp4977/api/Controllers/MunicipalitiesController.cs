using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MunicipalElections.Api.Data;
using MunicipalElections.Api.Dtos;

namespace MunicipalElections.Api.Controllers;

[ApiController]
[Route("api/municipalities")]
public class MunicipalitiesController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MunicipalityDto>>> GetAll()
    {
        var municipalities = await context.Municipalities
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .ToListAsync();
        return Ok(municipalities.Select(m => MunicipalityDto.From(m, Request)));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MunicipalityDto>> GetById(int id)
    {
        var municipality = await context.Municipalities
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.MunicipalityId == id);
        return municipality is null ? NotFound() : Ok(MunicipalityDto.From(municipality, Request));
    }
}
