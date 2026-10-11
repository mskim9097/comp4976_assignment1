namespace MunicipalElections.Web.Extensions;

// Logos and photos are stored as file names; this turns them into absolute URLs for the Web API (iOS app).
// Files live in wwwroot/images/logos and wwwroot/images/candidates.
public static class ImageUrls
{
    public static string? LogoUrl(this HttpRequest request, string? fileName) =>
        Absolute(request, "/images/logos/", fileName);

    public static string? PhotoUrl(this HttpRequest request, string? fileName) =>
        Absolute(request, "/images/candidates/", fileName);

    private static string? Absolute(HttpRequest request, string folder, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }
        if (fileName.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            fileName.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return fileName;
        }
        return $"{request.Scheme}://{request.Host}{folder}{Uri.EscapeDataString(fileName)}";
    }
}
