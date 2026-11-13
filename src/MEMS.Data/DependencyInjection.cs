using Microsoft.Extensions.DependencyInjection;

namespace MEMS.Data;

public static class DependencyInjection
{
    // One TenantContext per request (API) or per Blazor circuit (Admin).
    public static IServiceCollection AddMemsTenancy(this IServiceCollection services) =>
        services.AddScoped<TenantContext>();
}
