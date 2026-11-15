using MEMS.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MEMS.Data;

public static class MemsServiceRegistration
{
    // Service layer shared by the API and the Admin app (one implementation of the tenant rules).
    public static IServiceCollection AddMemsServices(this IServiceCollection services)
    {
        services.AddScoped<MunicipalityService>();
        services.AddScoped<PositionService>();
        services.AddScoped<CandidateService>();
        return services;
    }
}
