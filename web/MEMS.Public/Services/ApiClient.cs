using System.Net;
using System.Net.Http.Json;
using MEMS.Shared.Dtos;
using Microsoft.Extensions.Logging;

namespace MEMS.Public.Services;

public class ApiUnavailableException(string message, Exception? inner = null) : Exception(message, inner);

// Read-only client for the MEMS API. It only ever sends GET requests: nothing a visitor chooses is transmitted.
public class ApiClient(HttpClient http, ILogger<ApiClient> logger)
{
    // Turns an API-relative url (images) into an absolute one the browser can load.
    public string Absolute(string? relative) =>
        string.IsNullOrEmpty(relative) ? string.Empty : new Uri(http.BaseAddress!, relative).ToString();

    public async Task<List<MunicipalityDto>> GetMunicipalitiesAsync() =>
        await GetAsync<List<MunicipalityDto>>("api/municipalities") ?? [];

    // null = unknown municipality
    public Task<MunicipalityDto?> GetTenantAsync(string slug) => GetAsync<MunicipalityDto>($"api/{Escape(slug)}/municipality");

    public async Task<List<PositionDto>> GetPositionsAsync(string slug) =>
        await GetAsync<List<PositionDto>>($"api/{Escape(slug)}/positions") ?? [];

    public async Task<List<CandidateDto>> GetCandidatesAsync(string slug, string? search = null, int? positionId = null)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search.Trim())}");
        if (positionId is int id) query.Add($"positionId={id}");
        var url = $"api/{Escape(slug)}/candidates" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await GetAsync<List<CandidateDto>>(url) ?? [];
    }

    public Task<CandidateDto?> GetCandidateAsync(string slug, int id) =>
        GetAsync<CandidateDto>($"api/{Escape(slug)}/candidates/{id}");

    private static string Escape(string slug) => Uri.EscapeDataString(slug.ToLowerInvariant());

    private async Task<T?> GetAsync<T>(string url) where T : class
    {
        try
        {
            using var response = await http.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.NotFound) return null;
            if (!response.IsSuccessStatusCode)
                throw new ApiUnavailableException("The election service could not complete the request. Please try again.");
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (HttpRequestException ex)
        {
            // Browser console shows the exact address; the usual local cause is an untrusted https certificate or the API not running.
            logger.LogError(ex, "Could not reach the API at {Address}{Url}", http.BaseAddress, url);
            throw new ApiUnavailableException($"The election service is not reachable right now ({http.BaseAddress}). Please try again in a moment.", ex);
        }
    }
}
