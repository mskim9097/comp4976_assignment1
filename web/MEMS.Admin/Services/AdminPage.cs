using MEMS.Data;
using MEMS.Data.Services;
using Microsoft.AspNetCore.Components;

namespace MEMS.Admin.Services;

// Base class of every admin page. OwningComponentBase gives each page its own DI scope, hence its own DbContext:
// the safe way to use EF Core in Blazor Server (a context shared across a whole circuit is not thread safe).
public abstract class AdminPage : OwningComponentBase
{
    [Inject] protected AdminContext Admin { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected TenantScope Scope => Admin.Scope;
    protected MunicipalityService Municipalities => ScopedServices.GetRequiredService<MunicipalityService>();
    protected PositionService Positions => ScopedServices.GetRequiredService<PositionService>();
    protected CandidateService Candidates => ScopedServices.GetRequiredService<CandidateService>();

    // Images live in SQL Server and are streamed by the Admin app's own /media endpoints (tenant-checked).
    // The ?v= value is the image version from the API url, so a new upload shows immediately.
    protected static string? LogoSrc(MEMS.Shared.Dtos.MunicipalityDto m) =>
        m.LogoUrl is null ? null : $"media/municipalities/{m.MunicipalityId}?v={Version(m.LogoUrl)}";

    protected static string? PhotoSrc(MEMS.Shared.Dtos.CandidateDto c) =>
        c.PhotoUrl is null ? null : $"media/candidates/{c.MunicipalityId}/{c.CandidateId}?v={Version(c.PhotoUrl)}";

    private static string Version(string url)
    {
        var i = url.IndexOf("v=", StringComparison.Ordinal);
        return i < 0 ? "0" : url[(i + 2)..];
    }

    protected override void OnInitialized() => Admin.Changed += OnTenantChanged;

    private void OnTenantChanged() => _ = InvokeAsync(async () =>
    {
        await OnTenantChangedAsync();
        StateHasChanged();
    });

    // Pages that list tenant data reload here when the Super Admin switches municipality.
    protected virtual Task OnTenantChangedAsync() => Task.CompletedTask;

    protected override void Dispose(bool disposing)
    {
        if (disposing) Admin.Changed -= OnTenantChanged;
        base.Dispose(disposing);
    }
}
