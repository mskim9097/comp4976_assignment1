using MEMS.Shared.Dtos;
using MEMS.Data.Services;
using MEMS.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEMS.Api.Controllers;

// GET /api/{tenant}/municipality : the tenant's own details (this is what the tenant's public site loads first).
[AllowAnonymous]
public class TenantsController(MunicipalityService municipalities) : TenantControllerBase
{
    [HttpGet("/api/{tenant}/municipality")]
    [ProducesResponseType<MunicipalityDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MunicipalityDto>> Get(CancellationToken ct)
    {
        var dto = await municipalities.GetAsync(Scope, Tenant.MunicipalityId, ct);
        return dto is null ? NotFound() : Ok(dto);
    }
}
