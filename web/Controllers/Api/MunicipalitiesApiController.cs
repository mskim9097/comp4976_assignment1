using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MunicipalElections.Web.Data;
using MunicipalElections.Web.Dtos;

namespace MunicipalElections.Web.Controllers.Api;

[ApiController]
[Route("api/municipalities")]
public class MunicipalitiesApiController(ApplicationDbContext context) : ControllerBase
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
