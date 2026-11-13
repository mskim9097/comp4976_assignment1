using MEMS.Data.Entities;
using MEMS.Shared;
using MEMS.Shared.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MEMS.Data.Services;

// Administrator accounts of a municipality. Only a Super Admin (an unrestricted scope) creates or removes them.
public class AdminUserService(MemsDbContext db, UserManager<ApplicationUser> users, TenantContext tenant)
{
    public async Task<List<ApplicationUser>> ListAsync(int municipalityId, CancellationToken ct = default)
    {
        // Through the filtered set: a restricted caller cannot list the administrators of another municipality.
        if (!await db.Municipalities.AnyAsync(m => m.MunicipalityId == municipalityId, ct))
        {
            throw new NotFoundException("Municipality not found.");
        }
        return await db.Users.AsNoTracking().Where(u => u.MunicipalityId == municipalityId).OrderBy(u => u.UserName).ToListAsync(ct);
    }

    public async Task<ApplicationUser> CreateAsync(int municipalityId, CreateAdminRequest request)
    {
        EnsureUnrestricted();
        if (!await db.Municipalities.AnyAsync(m => m.MunicipalityId == municipalityId))
        {
            throw new NotFoundException("Municipality not found.");
        }

        var user = new ApplicationUser
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            EmailConfirmed = true,
            Name = Provinces.NormalizeName(request.Name),
            MunicipalityId = municipalityId
        };
        var result = await users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new RuleViolationException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        await users.AddToRoleAsync(user, Roles.MunicipalityAdmin);
        return user;
    }

    public async Task RemoveAsync(string userId)
    {
        EnsureUnrestricted();
        var user = await users.FindByIdAsync(userId);
        // Only municipality administrators can be removed here, never a Super Admin.
        if (user is null || user.MunicipalityId is null)
        {
            throw new NotFoundException("Administrator not found.");
        }
        var result = await users.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new RuleViolationException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }

    private void EnsureUnrestricted()
    {
        if (tenant.IsRestricted)
        {
            throw new ForbiddenException("Only a Super Admin can manage administrator accounts.");
        }
    }
}
