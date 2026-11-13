using System.Text.RegularExpressions;

namespace MEMS.Shared;

// Rules for municipality data entered by the Super Admin.
public static partial class Provinces
{
    // The Province field is a list, so "British Columbia", "BC" and "B.C." cannot all appear for the same province.
    public static readonly IReadOnlyList<string> All =
    [
        "Alberta", "British Columbia", "Manitoba", "New Brunswick", "Newfoundland and Labrador",
        "Northwest Territories", "Nova Scotia", "Nunavut", "Ontario", "Prince Edward Island",
        "Quebec", "Saskatchewan", "Yukon"
    ];

    public const string Default = "British Columbia";

    public static bool IsValid(string? value) => value is not null && All.Contains(value, StringComparer.Ordinal);

    // Trims the name and collapses repeated spaces ("  Maple   Ridge " -> "Maple Ridge").
    public static string NormalizeName(string? name) =>
        string.IsNullOrWhiteSpace(name) ? string.Empty : Whitespace().Replace(name.Trim(), " ");

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
