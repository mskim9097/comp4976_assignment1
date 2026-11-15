using System.Text.RegularExpressions;

namespace MEMS.Shared.Validation;

// Tenant slugs appear in public URLs (/burnaby) and API routes (/api/burnaby/candidates).
public static partial class SlugRules
{
    public const string Pattern = "^[a-z0-9]+(-[a-z0-9]+)*$";

    // Segments the applications already use, so a tenant can never shadow them.
    private static readonly HashSet<string> Reserved = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin", "api", "auth", "login", "logout", "swagger", "openapi", "health", "alive",
        "municipalities", "positions", "candidates", "guide", "error", "not-found", "_framework", "css", "js", "media"
    };

    public static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();

    public static bool IsReserved(string slug) => Reserved.Contains(slug);

    public static bool IsValid(string? slug) =>
        !string.IsNullOrEmpty(slug) && slug.Length <= 63 && SlugRegex().IsMatch(slug) && !IsReserved(slug);

    // "Pitt Meadows" -> "pitt-meadows" (used as a suggestion in the admin form).
    public static string Suggest(string? name)
    {
        var lower = (name ?? string.Empty).Trim().ToLowerInvariant();
        var dashed = NonSlug().Replace(lower, "-").Trim('-');
        return dashed;
    }

    [GeneratedRegex(Pattern)]
    private static partial Regex SlugRegex();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlug();
}
