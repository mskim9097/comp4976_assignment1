using System.Text.RegularExpressions;

namespace MEMS.Shared;

// The tenant slug is the municipality part of the public URL, for example "burnaby" in /burnaby.
public static partial class TenantSlug
{
    // Lower-case letters and digits, words separated by single hyphens.
    public const string Pattern = "^[a-z0-9]+(-[a-z0-9]+)*$";
    public const string ErrorMessage = "Use lower-case letters, digits and single hyphens only (for example: pitt-meadows).";

    [GeneratedRegex(Pattern)]
    private static partial Regex SlugRegex();

    // Words that are already used by the API routes (/api/auth, /api/municipalities ...) or by the apps.
    private static readonly HashSet<string> Reserved = new(StringComparer.Ordinal)
    {
        "api", "admin", "auth", "municipalities", "candidates", "offices", "elections", "positions",
        "swagger", "openapi", "health", "alive", "images", "uploads"
    };

    public static bool IsValid(string? slug) => !string.IsNullOrEmpty(slug) && SlugRegex().IsMatch(slug);

    public static bool IsReserved(string slug) => Reserved.Contains(slug);

    // Slug suggestion from a municipality name: "Pitt Meadows" -> "pitt-meadows".
    public static string FromName(string name)
    {
        var lower = Regex.Replace(name.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-");
        return lower.Trim('-');
    }
}
