namespace MEMS.Admin.Services;

// Photos and logos are files in the Api's images folder; the browser loads them from the Api (Api:BaseUrl).
public class ImageUrls(IConfiguration configuration)
{
    private readonly string baseUrl = (configuration["Api:BaseUrl"] ?? string.Empty).TrimEnd('/');

    public string? Logo(string? fileName) => Url("logos", fileName);

    public string? Photo(string? fileName) => Url("candidates", fileName);

    private string? Url(string folder, string? fileName) =>
        string.IsNullOrWhiteSpace(fileName) ? null : $"{baseUrl}/images/{folder}/{Uri.EscapeDataString(fileName)}";
}
