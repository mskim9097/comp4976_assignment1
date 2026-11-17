using System.ComponentModel.DataAnnotations;

namespace MEMS.Shared.Dtos;

// What the API returns. ElectionDate is a date-only value ("2026-10-17") so it never shifts with time zones.
public record MunicipalityDto(
    int MunicipalityId,
    string Name,
    string Slug,
    string Province,
    DateOnly ElectionDate,
    string? Description,
    string? LogoUrl,
    string ThemeColor,
    string? WelcomeMessage);

public record PositionDto(int PositionId, string Name, int SeatsAvailable, int MunicipalityId, string MunicipalityName);

public record CandidateDto(
    int CandidateId,
    string Name,
    string? Biography,
    string? PhotoUrl,
    string? Website,
    string? Email,
    int PositionId,
    string PositionName,
    int MunicipalityId,
    string MunicipalityName,
    DateTime CreatedDate);

// Election information of one city for the mobile app: GET /api/elections?city=Burnaby
public record ElectionDto(
    int MunicipalityId,
    string City,
    string Slug,
    DateOnly ElectionDate,
    string Province,
    IReadOnlyList<PositionDto> Offices);

// What the Admin app and API clients send. These attributes are the client-side and API-side validation.
public class MunicipalityRequest
{
    [Required(ErrorMessage = "Name is required."), StringLength(Limits.NameMax)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "The URL segment (slug) is required."), StringLength(Limits.SlugMax)]
    [RegularExpression(TenantSlug.Pattern, ErrorMessage = TenantSlug.ErrorMessage)]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Choose a province.")]
    public string Province { get; set; } = Provinces.Default;

    [Required(ErrorMessage = "Election date is required.")]
    public DateOnly? ElectionDate { get; set; }

    [StringLength(Limits.DescriptionMax)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Pick a theme colour.")]
    [RegularExpression("^#[0-9a-fA-F]{6}$", ErrorMessage = "Use a colour like #1f4e8c.")]
    public string ThemeColor { get; set; } = "#0d6efd";

    [StringLength(Limits.WelcomeMessageMax)]
    public string? WelcomeMessage { get; set; }
}

public class PositionRequest
{
    [Required(ErrorMessage = "Position name is required."), StringLength(Limits.NameMax)]
    public string Name { get; set; } = string.Empty;

    [Range(Limits.SeatsMin, Limits.SeatsMax, ErrorMessage = "Seats must be between 1 and 100.")]
    public int SeatsAvailable { get; set; } = 1;
}

public class CandidateRequest
{
    [Required(ErrorMessage = "Candidate name is required."), StringLength(Limits.NameMax)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Position is required.")]
    public int PositionId { get; set; }

    [StringLength(Limits.ProfileMax, ErrorMessage = "The biography can be at most {1} characters.")]
    public string? Biography { get; set; }

    [HttpUrl, StringLength(Limits.WebsiteMax)]
    public string? Website { get; set; }

    [EmailAddress(ErrorMessage = "Enter a valid email address."), StringLength(Limits.EmailMax)]
    public string? Email { get; set; }
}
