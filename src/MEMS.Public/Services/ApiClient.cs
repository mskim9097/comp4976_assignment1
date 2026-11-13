using System.Net.Http.Json;
using MEMS.Shared.Dtos;

namespace MEMS.Public.Services;

// The public site never touches the database: all data comes from the Web API, always for one tenant (/api/{slug}/...).
// Only GET requests are made here. The voter's choices are not sent anywhere (see GuideService).
public class ApiClient(HttpClient http)
{
    public async Task<List<MunicipalityDto>> GetMunicipalitiesAsync() =>
        await GetAsync<List<MunicipalityDto>>("api/municipalities") ?? [];

    // null when the municipality does not exist
    public Task<MunicipalityDto?> GetMunicipalityAsync(string slug) =>
        GetAsync<MunicipalityDto>($"api/municipalities/{Uri.EscapeDataString(slug)}");

    public async Task<List<PositionDto>> GetPositionsAsync(string slug) =>
        await GetAsync<List<PositionDto>>($"api/{Uri.EscapeDataString(slug)}/positions") ?? [];

    public async Task<List<CandidateDto>> GetCandidatesAsync(string slug, string? name = null, int? positionId = null)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(name))
        {
            query.Add($"name={Uri.EscapeDataString(name.Trim())}");
        }
        if (positionId is not null)
        {
            query.Add($"positionId={positionId}");
        }
        var url = $"api/{Uri.EscapeDataString(slug)}/candidates" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
        return await GetAsync<List<CandidateDto>>(url) ?? [];
    }

    // null when the candidate does not exist in this municipality
    public Task<CandidateDto?> GetCandidateAsync(string slug, int id) =>
        GetAsync<CandidateDto>($"api/{Uri.EscapeDataString(slug)}/candidates/{id}");

    private async Task<T?> GetAsync<T>(string url)
    {
        try
        {
            using var response = await http.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            throw new ApiUnavailableException("The election information could not be loaded. Check your connection and try again.", ex);
        }
    }
}

// A friendly message for the page; details never reach the user.
public class ApiUnavailableException(string message, Exception inner) : Exception(message, inner);
