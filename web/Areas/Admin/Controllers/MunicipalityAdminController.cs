using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MunicipalElections.Web.Data;
using MunicipalElections.Web.Models;
using MunicipalElections.Web.Services;

namespace MunicipalElections.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class MunicipalityAdminController(
    ApplicationDbContext context,
    AdminAccess adminAccess,
    IWebHostEnvironment environment,
    ILogger<MunicipalityAdminController> logger) : Controller
{
    public async Task<IActionResult> Index()
    {
        var municipalities = await GetManagedMunicipalitiesAsync();
        var model = new MunicipalityAdminDashboardViewModel();

        foreach (var municipality in municipalities)
        {
            var positions = await context.Positions
                .AsNoTracking()
                .Where(p => p.MunicipalityId == municipality.MunicipalityId)
                .OrderBy(p => p.Name)
                .ToListAsync();
            var positionIds = positions.Select(p => p.PositionId).ToList();
            var candidates = await context.Candidates
                .AsNoTracking()
                .Where(c => positionIds.Contains(c.PositionId))
                .OrderBy(c => c.Name)
                .ToListAsync();

            model.Municipalities.Add(new ManagedMunicipalityViewModel
            {
                Municipality = municipality,
                Positions = positions.Select(position => new ManagedPositionViewModel
                {
                    Position = position,
                    Candidates = candidates.Where(c => c.PositionId == position.PositionId).ToList()
                }).ToList()
            });
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> CreatePosition(int? municipalityId)
    {
        var municipalities = await GetManagedMunicipalitiesAsync();
        if (municipalities.Count == 0)
        {
            return Forbid();
        }

        ViewBag.Municipalities = MunicipalityOptions(municipalities);
        return View("PositionForm", new MunicipalityPositionFormViewModel
        {
            MunicipalityId = municipalities.Any(m => m.MunicipalityId == municipalityId)
                ? municipalityId!.Value
                : municipalities[0].MunicipalityId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePosition(MunicipalityPositionFormViewModel model)
    {
        var municipalities = await GetManagedMunicipalitiesAsync();
        if (!municipalities.Any(m => m.MunicipalityId == model.MunicipalityId))
        {
            ModelState.AddModelError(nameof(model.MunicipalityId), "You cannot manage positions for that municipality.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Municipalities = MunicipalityOptions(municipalities);
            return View("PositionForm", model);
        }

        context.Positions.Add(new Position
        {
            MunicipalityId = model.MunicipalityId,
            Name = model.Name.Trim(),
            SeatsAvailable = model.SeatsAvailable
        });
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(model.Name), "A position with this name already exists for the municipality.");
            ViewBag.Municipalities = MunicipalityOptions(municipalities);
            return View("PositionForm", model);
        }

        TempData["Success"] = "Position created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditPosition(int id)
    {
        var position = await FindManagedPositionAsync(id);
        if (position is null)
        {
            return NotFound();
        }

        ViewBag.Municipalities = MunicipalityOptions(await GetManagedMunicipalitiesAsync());
        return View("PositionForm", new MunicipalityPositionFormViewModel
        {
            PositionId = position.PositionId,
            MunicipalityId = position.MunicipalityId,
            Name = position.Name,
            SeatsAvailable = position.SeatsAvailable
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPosition(MunicipalityPositionFormViewModel model)
    {
        var position = await FindManagedPositionAsync(model.PositionId);
        if (position is null)
        {
            return NotFound();
        }

        var municipalities = await GetManagedMunicipalitiesAsync();
        if (!municipalities.Any(m => m.MunicipalityId == model.MunicipalityId))
        {
            ModelState.AddModelError(nameof(model.MunicipalityId), "You cannot manage positions for that municipality.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Municipalities = MunicipalityOptions(municipalities);
            return View("PositionForm", model);
        }

        position.MunicipalityId = model.MunicipalityId;
        position.Name = model.Name.Trim();
        position.SeatsAvailable = model.SeatsAvailable;
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(model.Name), "A position with this name already exists for the municipality.");
            ViewBag.Municipalities = MunicipalityOptions(municipalities);
            return View("PositionForm", model);
        }

        TempData["Success"] = "Position updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePosition(int id)
    {
        var position = await FindManagedPositionAsync(id);
        if (position is null)
        {
            return NotFound();
        }

        if (await context.Candidates.AnyAsync(c => c.PositionId == id))
        {
            TempData["Error"] = "A position cannot be deleted while it has candidates.";
            return RedirectToAction(nameof(Index));
        }

        context.Positions.Remove(position);
        await context.SaveChangesAsync();
        TempData["Success"] = "Position deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> CreateCandidate(int? positionId)
    {
        var positions = await GetManagedPositionsAsync();
        if (positions.Count == 0)
        {
            TempData["Error"] = "Create a position before adding candidates.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Positions = PositionOptions(positions);
        return View("CandidateForm", new MunicipalityCandidateFormViewModel
        {
            PositionId = positions.Any(p => p.PositionId == positionId)
                ? positionId!.Value
                : positions[0].PositionId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCandidate(MunicipalityCandidateFormViewModel model)
    {
        var positions = await GetManagedPositionsAsync();
        if (!positions.Any(p => p.PositionId == model.PositionId))
        {
            ModelState.AddModelError(nameof(model.PositionId), "Select a position in a municipality you manage.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Positions = PositionOptions(positions);
            return View("CandidateForm", model);
        }

        string? photoFileName;
        try
        {
            photoFileName = await ImageUploadStorage.SaveAsync(
                model.PhotoUpload, GetWebRootPath(), "candidates", HttpContext.RequestAborted);
        }
        catch (InvalidDataException exception)
        {
            ModelState.AddModelError(nameof(model.PhotoUpload), exception.Message);
            ViewBag.Positions = PositionOptions(positions);
            return View("CandidateForm", model);
        }

        var candidate = new Candidate
        {
            PositionId = model.PositionId,
            Name = model.Name.Trim(),
            Profile = model.Profile?.Trim(),
            Website = model.Website?.Trim(),
            Email = model.Email?.Trim(),
            CreatedDate = DateTime.UtcNow,
            PhotoFileName = photoFileName
        };
        context.Candidates.Add(candidate);
        var candidateSaved = false;
        try
        {
            await context.SaveChangesAsync();
            candidateSaved = true;
        }
        finally
        {
            if (!candidateSaved)
            {
                DeleteCandidateImage(photoFileName);
            }
        }

        TempData["Success"] = "Candidate created.";
        return RedirectToAction(nameof(Index));
    }

    // View one candidate (only candidates of municipalities the administrator manages).
    [HttpGet]
    public async Task<IActionResult> CandidateDetails(int id)
    {
        var candidate = await FindManagedCandidateAsync(id);
        if (candidate is null)
        {
            return NotFound();
        }

        await context.Entry(candidate.Position!).Reference(p => p.Municipality).LoadAsync();
        return View(candidate);
    }

    [HttpGet]
    public async Task<IActionResult> EditCandidate(int id)
    {
        var candidate = await FindManagedCandidateAsync(id);
        if (candidate is null)
        {
            return NotFound();
        }

        ViewBag.Positions = PositionOptions(await GetManagedPositionsAsync());
        return View("CandidateForm", new MunicipalityCandidateFormViewModel
        {
            CandidateId = candidate.CandidateId,
            PositionId = candidate.PositionId,
            Name = candidate.Name,
            Profile = candidate.Profile,
            Website = candidate.Website,
            Email = candidate.Email,
            CurrentPhotoFileName = candidate.PhotoFileName
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCandidate(MunicipalityCandidateFormViewModel model)
    {
        var candidate = await FindManagedCandidateAsync(model.CandidateId);
        if (candidate is null)
        {
            return NotFound();
        }

        var positions = await GetManagedPositionsAsync();
        if (!positions.Any(p => p.PositionId == model.PositionId))
        {
            ModelState.AddModelError(nameof(model.PositionId), "Select a position in a municipality you manage.");
        }

        if (!ModelState.IsValid)
        {
            model.CurrentPhotoFileName = candidate.PhotoFileName;
            ViewBag.Positions = PositionOptions(positions);
            return View("CandidateForm", model);
        }

        string? newPhotoFileName;
        try
        {
            newPhotoFileName = await ImageUploadStorage.SaveAsync(
                model.PhotoUpload, GetWebRootPath(), "candidates", HttpContext.RequestAborted);
        }
        catch (InvalidDataException exception)
        {
            ModelState.AddModelError(nameof(model.PhotoUpload), exception.Message);
            model.CurrentPhotoFileName = candidate.PhotoFileName;
            ViewBag.Positions = PositionOptions(positions);
            return View("CandidateForm", model);
        }

        var oldPhotoFileName = candidate.PhotoFileName;
        candidate.PositionId = model.PositionId;
        candidate.Name = model.Name.Trim();
        candidate.Profile = model.Profile?.Trim();
        candidate.Website = model.Website?.Trim();
        candidate.Email = model.Email?.Trim();
        if (newPhotoFileName is not null)
        {
            candidate.PhotoFileName = newPhotoFileName;
        }

        var candidateSaved = false;
        try
        {
            await context.SaveChangesAsync();
            candidateSaved = true;
        }
        finally
        {
            if (!candidateSaved)
            {
                DeleteCandidateImage(newPhotoFileName);
            }
        }

        if (newPhotoFileName is not null)
        {
            DeleteCandidateImage(oldPhotoFileName);
        }

        TempData["Success"] = "Candidate updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCandidate(int id)
    {
        var candidate = await FindManagedCandidateAsync(id);
        if (candidate is null)
        {
            return NotFound();
        }

        if (await context.SavedCandidates.AnyAsync(s => s.CandidateId == id))
        {
            TempData["Error"] = "A candidate cannot be deleted while voters have saved them.";
            return RedirectToAction(nameof(Index));
        }

        var photoFileName = candidate.PhotoFileName;
        context.Candidates.Remove(candidate);
        await context.SaveChangesAsync();
        DeleteCandidateImage(photoFileName);
        TempData["Success"] = "Candidate deleted.";
        return RedirectToAction(nameof(Index));
    }

    private Task<List<Municipality>> GetManagedMunicipalitiesAsync() =>
        adminAccess.GetManagedMunicipalitiesAsync(User);

    private async Task<List<Position>> GetManagedPositionsAsync()
    {
        var municipalityIds = (await GetManagedMunicipalitiesAsync()).Select(m => m.MunicipalityId).ToList();
        return await context.Positions
            .Include(p => p.Municipality)
            .Where(p => municipalityIds.Contains(p.MunicipalityId))
            .OrderBy(p => p.Municipality!.Name)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    private async Task<Position?> FindManagedPositionAsync(int id)
    {
        var municipalityIds = (await GetManagedMunicipalitiesAsync()).Select(m => m.MunicipalityId).ToList();
        return await context.Positions
            .FirstOrDefaultAsync(p => p.PositionId == id && municipalityIds.Contains(p.MunicipalityId));
    }

    private async Task<Candidate?> FindManagedCandidateAsync(int id)
    {
        var municipalityIds = (await GetManagedMunicipalitiesAsync()).Select(m => m.MunicipalityId).ToList();
        return await context.Candidates
            .Include(c => c.Position)
            .FirstOrDefaultAsync(c => c.CandidateId == id && municipalityIds.Contains(c.Position!.MunicipalityId));
    }

    private static List<SelectListItem> MunicipalityOptions(IEnumerable<Municipality> municipalities) =>
        municipalities.Select(m => new SelectListItem(m.Name, m.MunicipalityId.ToString())).ToList();

    private static List<SelectListItem> PositionOptions(IEnumerable<Position> positions) =>
        positions.Select(p => new SelectListItem($"{p.Municipality!.Name} - {p.Name}", p.PositionId.ToString())).ToList();

    private string GetWebRootPath() =>
        environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");

    private void DeleteCandidateImage(string? fileName)
    {
        try
        {
            ImageUploadStorage.DeleteUploaded(fileName, GetWebRootPath(), "candidates");
        }
        catch (IOException exception)
        {
            logger.LogError(exception, "Could not remove uploaded candidate image {FileName}.", fileName);
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogError(exception, "Could not remove uploaded candidate image {FileName}.", fileName);
        }
    }
}
