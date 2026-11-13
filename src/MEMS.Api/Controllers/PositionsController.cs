using MEMS.Api.Extensions;
using MEMS.Api.Tenancy;
using MEMS.Data;
using MEMS.Data.Services;
using MEMS.Shared;
using MEMS.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEMS.Api.Controllers;

// Positions of one municipality: /api/burnaby/positions. Reading is public; changes need an administrator of that municipality.
[ApiController]
[TenantScoped]
[Route("api/{slug}/positions")]
public class PositionsController(PositionService positions, TenantContext tenant) : ControllerBase
{
    private const string Administrators = $"{Roles.SuperAdmin},{Roles.MunicipalityAdmin}";

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PositionDto>>> GetAll(string slug, CancellationToken ct) =>
        Ok((await positions.ListAsync(tenant.MunicipalityId!.Value, ct)).Select(p => p.ToDto()));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PositionDto>> GetById(string slug, int id, CancellationToken ct) =>
        (await positions.GetAsync(id, ct)).ToDto();

    [Authorize(Roles = Administrators)]
    [HttpPost]
    public async Task<ActionResult<PositionDto>> Create(string slug, PositionRequest request, CancellationToken ct)
    {
        var created = await positions.CreateAsync(tenant.MunicipalityId!.Value, request, ct);
        return CreatedAtAction(nameof(GetById), new { slug, id = created.PositionId }, created.ToDto());
    }

    [Authorize(Roles = Administrators)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PositionDto>> Update(string slug, int id, PositionRequest request, CancellationToken ct) =>
        (await positions.UpdateAsync(id, request, ct)).ToDto();

    [Authorize(Roles = Administrators)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(string slug, int id, CancellationToken ct)
    {
        await positions.DeleteAsync(id, ct);
        return NoContent();
    }
}
