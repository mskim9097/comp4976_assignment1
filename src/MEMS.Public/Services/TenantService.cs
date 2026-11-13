using MEMS.Shared.Dtos;

namespace MEMS.Public.Services;

// Tenant resolution: the first part of the address (/burnaby) names the municipality; its details come from the Api.
public class TenantService(ApiClient api)
{
    private readonly Dictionary<string, MunicipalityDto> cache = new(StringComparer.OrdinalIgnoreCase);

    public async Task<MunicipalityDto?> GetAsync(string slug)
    {
        if (cache.TryGetValue(slug, out var cached))
        {
            return cached;
        }

        var municipality = await api.GetMunicipalityAsync(slug);
        if (municipality is not null)
        {
            cache[slug] = municipality;
        }
        return municipality;
    }

    // "/burnaby/candidates/3" -> "burnaby"; "/" -> null
    public static string? SlugFromPath(string path)
    {
        var first = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return first is null or "not-found" ? null : Uri.UnescapeDataString(first);
    }
}
