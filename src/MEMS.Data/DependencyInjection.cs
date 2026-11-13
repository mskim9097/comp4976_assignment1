using MEMS.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MEMS.Data;

public static class DependencyInjection
{
    // One TenantContext per request (Api) or per Blazor circuit (Admin), and the services that use it.
    // Uploads:Path is the folder for uploaded photos and logos (the Api serves it under /images).
    public static IServiceCollection AddMemsServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<TenantContext>();
        services.AddSingleton(new ImageFileStore(
            configuration["Uploads:Path"] ?? Path.Combine(AppContext.BaseDirectory, "uploads")));
        services.AddScoped<MunicipalityService>();
        services.AddScoped<PositionService>();
        services.AddScoped<CandidateService>();
        return services;
    }
}
