namespace MEMS.Admin.Services;

// Asks the API whether it is healthy. It exists to show Aspire service discovery working between two projects:
// the address "https+http://api" is resolved by the AppHost reference, nothing is hard-coded.
public class ApiStatusClient(HttpClient http, ILogger<ApiStatusClient> logger)
{
    public async Task<(bool Healthy, string Detail)> CheckAsync(CancellationToken ct = default)
    {
        try
        {
            using var response = await http.GetAsync("/health", ct);
            return response.IsSuccessStatusCode
                ? (true, "API is healthy")
                : (false, $"API answered {(int)response.StatusCode}");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            logger.LogWarning(ex, "API health check failed");
            return (false, "API is not reachable");
        }
    }
}
