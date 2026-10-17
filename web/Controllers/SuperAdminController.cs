using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MunicipalElections.Web.Data;
using MunicipalElections.Web.Models;

namespace MunicipalElections.Web.Controllers;

[Authorize(Roles = DbSeeder.SuperAdminRole)]
public class SuperAdminController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var municipalities = await context.Municipalities
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .ToListAsync();
        var model = new SuperAdminIndexViewModel();

        foreach (var municipality in municipalities)
        {
            model.Municipalities.Add(new MunicipalityAdminViewModel
            {
                Municipality = municipality,
                Admins = (await userManager.GetUsersInRoleAsync(municipality.AdminRoleName)).ToList(),
                PositionCount = await context.Positions.CountAsync(p => p.MunicipalityId == municipality.MunicipalityId)
            });
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult CreateMunicipality() => View("MunicipalityForm", new MunicipalityFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMunicipality(MunicipalityFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("MunicipalityForm", model);
        }

        var roleName = DbSeeder.AdminRoleNameFor(model.Name.Trim());
        var createdRole = false;
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!roleResult.Succeeded)
            {
                AddIdentityErrors(roleResult);
                return View("MunicipalityForm", model);
            }
            createdRole = true;
        }

        var municipality = new Municipality
        {
            Name = model.Name.Trim(),
            Province = model.Province.Trim(),
            ElectionDate = model.ElectionDate,
            Description = model.Description?.Trim(),
            AdminRoleName = roleName
        };
        context.Municipalities.Add(municipality);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (createdRole)
            {
                await roleManager.DeleteAsync(await roleManager.FindByNameAsync(roleName) ??
                    throw new InvalidOperationException($"Role '{roleName}' disappeared while creating a municipality."));
            }

            ModelState.AddModelError(nameof(model.Name), "A municipality with this name already exists.");
            return View("MunicipalityForm", model);
        }

        TempData["Success"] = $"Municipality '{municipality.Name}' created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditMunicipality(int id)
    {
        var municipality = await context.Municipalities.FindAsync(id);
        if (municipality is null)
        {
            return NotFound();
        }

        return View("MunicipalityForm", new MunicipalityFormViewModel
        {
            MunicipalityId = municipality.MunicipalityId,
            Name = municipality.Name,
            Province = municipality.Province,
            ElectionDate = municipality.ElectionDate,
            Description = municipality.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMunicipality(MunicipalityFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("MunicipalityForm", model);
        }

        var municipality = await context.Municipalities.FindAsync(model.MunicipalityId);
        if (municipality is null)
        {
            return NotFound();
        }

        municipality.Name = model.Name.Trim();
        municipality.Province = model.Province.Trim();
        municipality.ElectionDate = model.ElectionDate;
        municipality.Description = model.Description?.Trim();

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(model.Name), "A municipality with this name already exists.");
            return View("MunicipalityForm", model);
        }

        TempData["Success"] = $"Municipality '{municipality.Name}' updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> CreateAdmin(int? municipalityId)
    {
        ViewBag.Municipalities = await GetMunicipalityOptionsAsync();
        return View(new CreateMunicipalityAdminViewModel { MunicipalityId = municipalityId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAdmin(CreateMunicipalityAdminViewModel model)
    {
        var municipality = await context.Municipalities
            .FirstOrDefaultAsync(m => m.MunicipalityId == model.MunicipalityId);
        if (municipality is null)
        {
            ModelState.AddModelError(nameof(model.MunicipalityId), "Select a valid municipality.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Municipalities = await GetMunicipalityOptionsAsync();
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName.Trim(),
            Email = model.Email.Trim(),
            Name = model.Name.Trim(),
            EmailConfirmed = true
        };
        var createResult = await userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            ViewBag.Municipalities = await GetMunicipalityOptionsAsync();
            return View(model);
        }

        var roleResult = await userManager.AddToRoleAsync(user, municipality!.AdminRoleName);
        if (!roleResult.Succeeded)
        {
            AddIdentityErrors(roleResult);
            var deleteResult = await userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                AddIdentityErrors(deleteResult);
            }
            ViewBag.Municipalities = await GetMunicipalityOptionsAsync();
            return View(model);
        }

        TempData["Success"] = $"Admin account '{user.UserName}' created for {municipality.Name}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAdmin(string userId, int municipalityId)
    {
        var user = await userManager.FindByIdAsync(userId);
        var municipality = await context.Municipalities.FindAsync(municipalityId);
        if (user is null || municipality is null)
        {
            return NotFound();
        }

        var result = await userManager.RemoveFromRoleAsync(user, municipality.AdminRoleName);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            TempData["Error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        }
        else
        {
            TempData["Success"] = $"Admin access removed for '{user.UserName}' in {municipality.Name}.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMunicipality(int id)
    {
        var municipality = await context.Municipalities.FindAsync(id);
        if (municipality is null)
        {
            return NotFound();
        }

        if (await context.Positions.AnyAsync(p => p.MunicipalityId == id))
        {
            TempData["Error"] = $"'{municipality.Name}' cannot be deleted because it has positions. Remove its election data first.";
            return RedirectToAction(nameof(Index));
        }

        var role = await roleManager.FindByNameAsync(municipality.AdminRoleName);
        if (role is not null)
        {
            foreach (var user in await userManager.GetUsersInRoleAsync(municipality.AdminRoleName))
            {
                var removeResult = await userManager.RemoveFromRoleAsync(user, municipality.AdminRoleName);
                if (!removeResult.Succeeded)
                {
                    TempData["Error"] = $"Could not remove admin access for '{user.UserName}': " +
                        string.Join("; ", removeResult.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Index));
                }
            }

            var deleteRoleResult = await roleManager.DeleteAsync(role);
            if (!deleteRoleResult.Succeeded)
            {
                TempData["Error"] = string.Join("; ", deleteRoleResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }
        }

        context.Municipalities.Remove(municipality);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = $"'{municipality.Name}' could not be deleted because it is still referenced by election data.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = $"Municipality '{municipality.Name}' deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>> GetMunicipalityOptionsAsync() =>
        await context.Municipalities
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .Select(m => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(m.Name, m.MunicipalityId.ToString()))
            .ToListAsync();

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
