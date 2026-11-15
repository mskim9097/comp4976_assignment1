using MEMS.Data;
using MEMS.Data.Services;
using MEMS.Shared.Dtos;
using Microsoft.AspNetCore.Components.Authorization;

namespace MEMS.Admin.Services;

// Per-circuit state: who is signed in (as a TenantScope) and which municipality the admin is working in.
// A Municipality Admin is locked to their own tenant; the Super Admin picks one from the menu.
public class AdminContext(AuthenticationStateProvider authentication, IServiceScopeFactory scopes)
{
    private Task? _loading;

    public TenantScope Scope { get; private set; } = TenantScope.Anonymous;
    public List<MunicipalityDto> Tenants { get; private set; } = [];
    public int? ActiveTenantId { get; private set; }
    public MunicipalityDto? ActiveTenant => Tenants.FirstOrDefault(t => t.MunicipalityId == ActiveTenantId);

    public event Action? Changed;

    // Safe to call from every page: the work happens once.
    public Task EnsureLoadedAsync() => _loading ??= LoadAsync();

    public async Task RefreshTenantsAsync()
    {
        var state = await authentication.GetAuthenticationStateAsync();
        Scope = TenantScope.FromPrincipal(state.User);
        if (!Scope.IsAuthenticated)
        {
            // Nothing to load for the sign-in page.
            Tenants = [];
            ActiveTenantId = null;
            return;
        }

        // The service uses its own scope (own DbContext) so it never races with a page's context.
        using var scope = scopes.CreateScope();
        Tenants = await scope.ServiceProvider.GetRequiredService<MunicipalityService>().ListAsync(Scope);

        if (ActiveTenantId is null || Tenants.All(t => t.MunicipalityId != ActiveTenantId))
        {
            ActiveTenantId = Scope.IsMunicipalityAdmin ? Scope.MunicipalityId : Tenants.FirstOrDefault()?.MunicipalityId;
        }
        Changed?.Invoke();
    }

    public void SelectTenant(int municipalityId)
    {
        // Only a tenant the admin may manage can become active.
        if (!Scope.CanManage(municipalityId) || ActiveTenantId == municipalityId) return;
        ActiveTenantId = municipalityId;
        Changed?.Invoke();
    }

    private Task LoadAsync() => RefreshTenantsAsync();
}
