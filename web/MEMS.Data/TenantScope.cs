using System.Security.Claims;
using MEMS.Shared;

namespace MEMS.Data;

// Who is asking, and which tenant (if any) they belong to. Built from the authenticated principal (cookie in the Admin app,
// JWT in the API) and passed to every service call, so the service layer enforces isolation no matter which UI called it.
public sealed record TenantScope(bool IsAuthenticated, bool IsSuperAdmin, int? MunicipalityId)
{
    public static TenantScope Anonymous { get; } = new(false, false, null);

    public static TenantScope FromPrincipal(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true) return Anonymous;

        int? tenantId = int.TryParse(user.FindFirstValue(MemsClaims.MunicipalityId), out var id) ? id : null;
        return new TenantScope(true, user.IsInRole(MemsRoles.SuperAdmin), tenantId);
    }

    // May this caller change data in the tenant? (Super Admin: any tenant; Municipality Admin: only their own.)
    public bool CanManage(int municipalityId) =>
        IsAuthenticated && (IsSuperAdmin || MunicipalityId == municipalityId);

    // May this caller read the tenant through the API? Anonymous visitors read public data of any tenant (that is what each
    // tenant's public site does), but a signed-in Municipality Admin is confined to their own tenant.
    public bool CanSee(int municipalityId) =>
        !IsAuthenticated || IsSuperAdmin || MunicipalityId == municipalityId;

    public bool IsMunicipalityAdmin => IsAuthenticated && !IsSuperAdmin;
}
