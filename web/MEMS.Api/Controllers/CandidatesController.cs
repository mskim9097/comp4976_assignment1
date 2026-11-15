using MEMS.Api.Infrastructure;
using MEMS.Data.Services;
using MEMS.Shared;
using MEMS.Shared.Dtos;
using MEMS.Shared.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEMS.Api.Controllers;

[Route("api/{tenant}/candidates")]
public class CandidatesController(CandidateService candidates) : TenantControllerBase
{
    // GET /api/burnaby/candidates?search=ali&positionId=3  : only Burnaby candidates, ever.
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<List<CandidateDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CandidateDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] int? positionId, CancellationToken ct) =>
        Ok(await candidates.ListAsync(Tenant.MunicipalityId, search, positionId, ct));

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType<CandidateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CandidateDto>> GetById(int id, CancellationToken ct)
    {
        // A candidate id that belongs to another tenant is simply "not found" here.
        var dto = await candidates.GetAsync(Tenant.MunicipalityId, id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [Authorize(Policy = MemsPolicies.AnyAdmin)]
    [HttpPost]
    [ProducesResponseType<CandidateDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CandidateRequest request, CancellationToken ct) =>
        (await candidates.CreateAsync(Scope, Tenant.MunicipalityId, request, ct)).ToActionResult(this);

    [Authorize(Policy = MemsPolicies.AnyAdmin)]
    [HttpPut("{id:int}")]
    [ProducesResponseType<CandidateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CandidateRequest request, CancellationToken ct) =>
        (await candidates.UpdateAsync(Scope, Tenant.MunicipalityId, id, request, ct)).ToActionResult(this);

    [Authorize(Policy = MemsPolicies.AnyAdmin)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        (await candidates.DeleteAsync(Scope, Tenant.MunicipalityId, id, ct)).ToActionResult(this);

    [AllowAnonymous]
    [HttpGet("{id:int}/photo")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPhoto(int id, CancellationToken ct)
    {
        var photo = await candidates.GetPhotoAsync(Tenant.MunicipalityId, id, ct);
        if (photo is null) return NotFound();

        SetImageHeaders(Response);
        return File(photo.Value.Data, photo.Value.ContentType);
    }

    [Authorize(Policy = MemsPolicies.AnyAdmin)]
    [HttpPut("{id:int}/photo")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ImageRules.MaxBytes + 64 * 1024)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile file, CancellationToken ct)
    {
        if (file.Length is 0 or > ImageRules.MaxBytes)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["file"] = ["Images must be 2 MB or smaller."] }));

        await using var stream = file.OpenReadStream();
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, ct);
        return (await candidates.SetPhotoAsync(Scope, Tenant.MunicipalityId, id, file.FileName, buffer.ToArray(), ct)).ToActionResult(this);
    }
}
