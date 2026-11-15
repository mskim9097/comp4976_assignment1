using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MEMS.Data.Seed;

// Sample images for the seed data are generated (initials on a colored square) so the repository needs no binary files.
// Uploads from administrators are restricted to PNG/JPEG/GIF/WebP; these SVGs are trusted, generated content only.
internal static class SeedImages
{
    public const string SvgContentType = "image/svg+xml";

    private static readonly string[] Palette =
        ["#1d4ed8", "#0f766e", "#b45309", "#9333ea", "#be123c", "#166534", "#0e7490", "#7c2d12", "#4338ca", "#a21caf"];

    public static byte[] Avatar(string name)
    {
        var colorIndex = SHA256.HashData(Encoding.UTF8.GetBytes(name))[0] % Palette.Length;
        return Svg(Initials(name), Palette[colorIndex], 90);
    }

    public static byte[] Logo(string name, string color) => Svg(Initials(name), color, 80);

    private static string Initials(string name)
    {
        var parts = Regex.Split(name.Trim(), @"[\s-]+").Where(p => p.Length > 0).ToArray();
        return parts.Length == 0 ? "?" : (parts[0][..1] + (parts.Length > 1 ? parts[^1][..1] : string.Empty)).ToUpperInvariant();
    }

    private static byte[] Svg(string text, string color, int fontSize)
    {
        var svg = $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 200 200\" width=\"200\" height=\"200\">" +
                  $"<rect width=\"200\" height=\"200\" fill=\"{color}\"/>" +
                  $"<text x=\"100\" y=\"{100 + fontSize / 3}\" font-family=\"Arial, Helvetica, sans-serif\" font-size=\"{fontSize}\" " +
                  $"font-weight=\"bold\" fill=\"#ffffff\" text-anchor=\"middle\">{System.Security.SecurityElement.Escape(text)}</text></svg>";
        return Encoding.UTF8.GetBytes(svg);
    }
}
