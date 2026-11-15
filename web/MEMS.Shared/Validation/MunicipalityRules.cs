using System.Text.RegularExpressions;

namespace MEMS.Shared.Validation;

public static partial class MunicipalityRules
{
    public static readonly IReadOnlyList<string> Provinces =
    [
        "Alberta", "British Columbia", "Manitoba", "New Brunswick", "Newfoundland and Labrador",
        "Northwest Territories", "Nova Scotia", "Nunavut", "Ontario", "Prince Edward Island",
        "Quebec", "Saskatchewan", "Yukon"
    ];

    public const string DefaultProvince = "British Columbia";
    public const string ColorPattern = "^#[0-9A-Fa-f]{6}$";

    public static bool IsProvince(string? value) => value is not null && Provinces.Contains(value, StringComparer.Ordinal);

    // "  Maple   Ridge " -> "Maple Ridge"
    public static string NormalizeName(string? name) =>
        string.IsNullOrWhiteSpace(name) ? string.Empty : Whitespace().Replace(name.Trim(), " ");

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
