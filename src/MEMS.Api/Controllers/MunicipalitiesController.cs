using MEMS.Api.Extensions;
using MEMS.Data.Services;
using MEMS.Shared;
using MEMS.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEMS.Api.Controllers;

// Municipalities (tenants). Reading is public; creating, changing and deleting is for the Super Admin.
[ApiController]
[Route("api/municipalities")]
public class MunicipalitiesController(MunicipalityService municipalities) : ControllerBase
{
    private const string SuperAdminOnly = Roles.SuperAdmin;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MunicipalityDto>>> GetAll(CancellationToken ct) =>
        Ok((await municipalities.ListAsync(ct)).Select(m => m.ToDto(Request)));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MunicipalityDto>> GetById(int id, CancellationToken ct) =>
        (await municipalities.GetAsync(id, ct)).ToDto(Request);

    [HttpGet("{slug}")]
    public async Task<ActionResult<MunicipalityDto>> GetBySlug(string slug, CancellationToken ct) =>
        (await municipalities.GetBySlugAsync(slug, ct)).ToDto(Request);

    [Authorize(Roles = SuperAdminOnly)]
    [HttpPost]
    public async Task<ActionResult<MunicipalityDto>> Create(MunicipalityRequest request, CancellationToken ct)
    {
        var created = await municipalities.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.MunicipalityId }, created.ToDto(Request));
    }

    [Authorize(Roles = SuperAdminOnly)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<MunicipalityDto>> Update(int id, MunicipalityRequest request, CancellationToken ct) =>
        (await municipalities.UpdateAsync(id, request, ct)).ToDto(Request);

    [Authorize(Roles = SuperAdminOnly)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await municipalities.DeleteAsync(id, ct);
        return NoContent();
    }

    // multipart/form-data with one file field called "file"
    [Authorize(Roles = SuperAdminOnly)]
    [HttpPost("{id:int}/logo")]
    public async Task<ActionResult<MunicipalityDto>> UploadLogo(int id, IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        var updated = await municipalities.SetLogoAsync(id, file.FileName, file.ContentType, file.Length, stream, ct);
        return updated.ToDto(Request);
    }
}
