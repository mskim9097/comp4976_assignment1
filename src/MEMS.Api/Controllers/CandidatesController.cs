using MEMS.Api.Extensions;
using MEMS.Api.Tenancy;
using MEMS.Data;
using MEMS.Data.Services;
using MEMS.Shared;
using MEMS.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEMS.Api.Controllers;

// Candidates of one municipality: /api/burnaby/candidates. Reading is public; changes need an administrator of that municipality.
[ApiController]
[TenantScoped]
[Route("api/{slug}/candidates")]
public class CandidatesController(CandidateService candidates, TenantContext tenant) : ControllerBase
{
    private const string Administrators = $"{Roles.SuperAdmin},{Roles.MunicipalityAdmin}";

    // Optional ?name= (contains, ignoring case) and ?positionId=
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CandidateDto>>> GetAll(
        string slug, [FromQuery] string? name, [FromQuery] int? positionId, CancellationToken ct) =>
        Ok((await candidates.ListAsync(tenant.MunicipalityId!.Value, name, positionId, ct)).Select(c => c.ToDto(Request)));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CandidateDto>> GetById(string slug, int id, CancellationToken ct) =>
        (await candidates.GetAsync(id, ct)).ToDto(Request);

    [Authorize(Roles = Administrators)]
    [HttpPost]
    public async Task<ActionResult<CandidateDto>> Create(string slug, CandidateRequest request, CancellationToken ct)
    {
        var created = await candidates.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { slug, id = created.CandidateId }, created.ToDto(Request));
    }

    [Authorize(Roles = Administrators)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CandidateDto>> Update(string slug, int id, CandidateRequest request, CancellationToken ct) =>
        (await candidates.UpdateAsync(id, request, ct)).ToDto(Request);

    [Authorize(Roles = Administrators)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(string slug, int id, CancellationToken ct)
    {
        await candidates.DeleteAsync(id, ct);
        return NoContent();
    }

    // multipart/form-data with one file field called "file"
    [Authorize(Roles = Administrators)]
    [HttpPost("{id:int}/photo")]
    public async Task<ActionResult<CandidateDto>> UploadPhoto(string slug, int id, IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        var updated = await candidates.SetPhotoAsync(id, file.FileName, file.ContentType, file.Length, stream, ct);
        return updated.ToDto(Request);
    }
}
