using MEMS.Api.Infrastructure;
using MEMS.Data.Services;
using MEMS.Shared;
using MEMS.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEMS.Api.Controllers;

[Route("api/{tenant}/positions")]
public class PositionsController(PositionService positions) : TenantControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<List<PositionDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PositionDto>>> GetAll(CancellationToken ct) =>
        Ok(await positions.ListAsync(Tenant.MunicipalityId, ct));

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType<PositionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PositionDto>> GetById(int id, CancellationToken ct)
    {
        var dto = await positions.GetAsync(Tenant.MunicipalityId, id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [Authorize(Policy = MemsPolicies.AnyAdmin)]
    [HttpPost]
    [ProducesResponseType<PositionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] PositionRequest request, CancellationToken ct) =>
        (await positions.CreateAsync(Scope, Tenant.MunicipalityId, request, ct)).ToActionResult(this);

    [Authorize(Policy = MemsPolicies.AnyAdmin)]
    [HttpPut("{id:int}")]
    [ProducesResponseType<PositionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] PositionRequest request, CancellationToken ct) =>
        (await positions.UpdateAsync(Scope, Tenant.MunicipalityId, id, request, ct)).ToActionResult(this);

    [Authorize(Policy = MemsPolicies.AnyAdmin)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        (await positions.DeleteAsync(Scope, Tenant.MunicipalityId, id, ct)).ToActionResult(this);
}
