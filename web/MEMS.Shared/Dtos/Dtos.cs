using System.ComponentModel.DataAnnotations;
using MEMS.Shared.Validation;

namespace MEMS.Shared.Dtos;

// ---------- Responses ----------

// LogoUrl is relative to the API (e.g. /api/municipalities/3/logo?v=2); clients prefix their API base address.
public record MunicipalityDto(
    int MunicipalityId, string Name, string Slug, string Province, DateOnly ElectionDate,
    string? Description, string ThemeColor, string? LogoUrl, int PositionCount, int CandidateCount);

public record PositionDto(int PositionId, string Name, int SeatsAvailable, int MunicipalityId, int CandidateCount);

public record CandidateDto(
    int CandidateId, string Name, string? Profile, string? Website, string? Email,
    int PositionId, string PositionName, int MunicipalityId, string? PhotoUrl, DateTime CreatedUtc);

public record DashboardDto(int Municipalities, int Positions, int Candidates);

public record TokenResponse(string Token, DateTime ExpiresUtc, string UserName, string Role, int? MunicipalityId);

// ---------- Requests (validated on the client by Blazor, on the server by [ApiController], and by the database) ----------

public class MunicipalityRequest
{
    [Required(ErrorMessage = "Municipality name is required."), StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "The URL segment (slug) is required."), StringLength(63, MinimumLength = 2)]
    [RegularExpression(SlugRules.Pattern, ErrorMessage = "Use lowercase letters, numbers and single hyphens only (for example: pitt-meadows).")]
    [Display(Name = "URL segment (slug)")]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select a province or territory."), StringLength(50)]
    public string Province { get; set; } = MunicipalityRules.DefaultProvince;

    [Display(Name = "Election date")]
    public DateOnly ElectionDate { get; set; } = new(2026, 10, 17);

    [StringLength(2000, ErrorMessage = "The description can be at most {1} characters.")]
    public string? Description { get; set; }

    [Required, RegularExpression(MunicipalityRules.ColorPattern, ErrorMessage = "Use a color such as #0d6efd.")]
    [Display(Name = "Theme color")]
    public string ThemeColor { get; set; } = "#0d6efd";
}

public class PositionRequest
{
    [Required(ErrorMessage = "Position name is required."), StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "Seats available must be between {1} and {2}.")]
    [Display(Name = "Seats available")]
    public int SeatsAvailable { get; set; } = 1;
}

public class CandidateRequest
{
    [Required(ErrorMessage = "Candidate name is required."), StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Position is required.")]
    [Display(Name = "Position")]
    public int PositionId { get; set; }

    [StringLength(2000, ErrorMessage = "The biography / profile can be at most {1} characters.")]
    [Display(Name = "Biography / profile")]
    public string? Profile { get; set; }

    [HttpUrl, StringLength(500)]
    [Display(Name = "Website URL")]
    public string? Website { get; set; }

    [EmailAddress(ErrorMessage = "Enter a valid email address."), StringLength(200)]
    [Display(Name = "Email address")]
    public string? Email { get; set; }
}

public class LoginRequest
{
    [Required] public string UserName { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class CreateAdminRequest
{
    [Required, StringLength(256, MinimumLength = 3)]
    [Display(Name = "User name")]
    public string UserName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Full name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8, ErrorMessage = "The password must be at least {2} characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
