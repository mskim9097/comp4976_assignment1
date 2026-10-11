namespace MunicipalElections.Api.Extensions;

public static class HttpRequestExtensions
{
    // Image paths are stored relative to the API host; clients (iOS) need absolute URLs.
    public static string? ToAbsoluteUrl(this HttpRequest request, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }
        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }
        return $"{request.Scheme}://{request.Host}{path}";
    }
}
