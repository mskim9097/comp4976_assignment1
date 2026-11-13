using MEMS.Api.Extensions;
using MEMS.Api.Tenancy;
using MEMS.Data;
using MEMS.Data.Services;
using MEMS.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace MEMS.Api.Controllers;

// Read-only endpoints for the mobile app. The city is required (?city=Burnaby, name or slug) and every answer
// is limited to that municipality, the same isolation as /api/{slug}/... uses.
[ApiController]
[TenantScoped]
public class CityController(
    MunicipalityService municipalities, PositionService positions, CandidateService candidates, TenantContext tenant) : ControllerBase
{
    // GET /api/candidates?city=Burnaby&name=an&positionId=3
    [HttpGet("/api/candidates")]
    [HttpGet("/api/candidates/search")]
    public async Task<ActionResult<IEnumerable<CandidateDto>>> GetCandidates(
        [FromQuery] string city, [FromQuery] string? name, [FromQuery] int? positionId, CancellationToken ct) =>
        Ok((await candidates.ListAsync(tenant.MunicipalityId!.Value, name, positionId, ct)).Select(c => c.ToDto(Request)));

    [HttpGet("/api/candidates/{id:int}")]
    public async Task<ActionResult<CandidateDto>> GetCandidate([FromQuery] string city, int id, CancellationToken ct) =>
        (await candidates.GetAsync(id, ct)).ToDto(Request);

    // "Offices" are the elected positions of the city.
    [HttpGet("/api/offices")]
    public async Task<ActionResult<IEnumerable<PositionDto>>> GetOffices([FromQuery] string city, CancellationToken ct) =>
        Ok((await positions.ListAsync(tenant.MunicipalityId!.Value, ct)).Select(p => p.ToDto()));

    [HttpGet("/api/offices/{id:int}")]
    public async Task<ActionResult<PositionDto>> GetOffice([FromQuery] string city, int id, CancellationToken ct) =>
        (await positions.GetAsync(id, ct)).ToDto();

    // The election of the city: date and offices.
    [HttpGet("/api/elections")]
    public async Task<ActionResult<ElectionDto>> GetElection([FromQuery] string city, CancellationToken ct)
    {
        var municipality = await municipalities.GetAsync(tenant.MunicipalityId!.Value, ct);
        var offices = await positions.ListAsync(municipality.MunicipalityId, ct);
        return new ElectionDto(
            municipality.MunicipalityId, municipality.Name, municipality.Slug, municipality.ElectionDate,
            municipality.Province, [.. offices.Select(p => p.ToDto())]);
    }
}
