using MEMS.Api.Infrastructure;
using MEMS.Data.Services;
using MEMS.Shared;
using MEMS.Shared.Dtos;
using MEMS.Shared.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEMS.Api.Controllers;

// Tenant (municipality) endpoints. Reading is public; create / edit / delete is Super Admin only.
[ApiController]
[Route("api/municipalities")]
public class MunicipalitiesController(MunicipalityService municipalities) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<List<MunicipalityDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MunicipalityDto>>> GetAll(CancellationToken ct) =>
        Ok(await municipalities.ListAsync(User.ToScope(), ct));

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType<MunicipalityDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MunicipalityDto>> GetById(int id, CancellationToken ct)
    {
        var dto = await municipalities.GetAsync(User.ToScope(), id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [Authorize(Policy = MemsPolicies.SuperAdmin)]
    [HttpPost]
    [ProducesResponseType<MunicipalityDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] MunicipalityRequest request, CancellationToken ct) =>
        (await municipalities.CreateAsync(User.ToScope(), request, ct)).ToActionResult(this);

    [Authorize(Policy = MemsPolicies.SuperAdmin)]
    [HttpPut("{id:int}")]
    [ProducesResponseType<MunicipalityDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] MunicipalityRequest request, CancellationToken ct) =>
        (await municipalities.UpdateAsync(User.ToScope(), id, request, ct)).ToActionResult(this);

    [Authorize(Policy = MemsPolicies.SuperAdmin)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        (await municipalities.DeleteAsync(User.ToScope(), id, ct)).ToActionResult(this);

    [AllowAnonymous]
    [HttpGet("{id:int}/logo")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLogo(int id, CancellationToken ct)
    {
        var logo = await municipalities.GetLogoAsync(id, ct);
        if (logo is null) return NotFound();

        Response.Headers.CacheControl = "public,max-age=86400";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers.ContentSecurityPolicy = "default-src 'none'; style-src 'unsafe-inline'; sandbox";
        return File(logo.Value.Data, logo.Value.ContentType);
    }

    [Authorize(Policy = MemsPolicies.SuperAdmin)]
    [HttpPut("{id:int}/logo")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ImageRules.MaxBytes + 64 * 1024)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadLogo(int id, IFormFile file, CancellationToken ct)
    {
        if (file.Length is 0 or > ImageRules.MaxBytes)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["file"] = ["Images must be 2 MB or smaller."] }));

        await using var stream = file.OpenReadStream();
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, ct);
        return (await municipalities.SetLogoAsync(User.ToScope(), id, file.FileName, buffer.ToArray(), ct)).ToActionResult(this);
    }
}
