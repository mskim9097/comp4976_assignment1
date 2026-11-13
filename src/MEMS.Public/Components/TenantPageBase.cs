using MEMS.Public.Services;
using MEMS.Shared.Dtos;
using Microsoft.AspNetCore.Components;

namespace MEMS.Public.Components;

// Base of every page that belongs to one municipality (/{slug}/...). It resolves the tenant from the address and
// reloads when the address changes to another municipality.
public abstract class TenantPageBase : ComponentBase
{
    [Inject] protected TenantService Tenants { get; set; } = default!;

    [Parameter] public string Slug { get; set; } = string.Empty;

    protected MunicipalityDto? Tenant { get; private set; }
    protected string? Error { get; set; }
    protected bool NotFound { get; private set; }
    protected bool Loading { get; set; } = true;

    // What the data of the page depends on. A page can add more (the candidate id).
    protected virtual string LoadKey => Slug;

    private string? loadedKey;

    protected override async Task OnParametersSetAsync()
    {
        if (loadedKey == LoadKey)
        {
            return;
        }

        loadedKey = LoadKey;
        Loading = true;
        Error = null;
        NotFound = false;
        try
        {
            Tenant = await Tenants.GetAsync(Slug);
            if (Tenant is null)
            {
                NotFound = true;
            }
            else
            {
                await LoadAsync();
            }
        }
        catch (ApiUnavailableException ex)
        {
            Error = ex.Message;
        }
        Loading = false;
    }

    // Loads the data of the page for Tenant.
    protected virtual Task LoadAsync() => Task.CompletedTask;
}
