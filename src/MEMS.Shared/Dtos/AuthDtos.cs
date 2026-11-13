using System.ComponentModel.DataAnnotations;

namespace MEMS.Shared.Dtos;

public class RegisterRequest
{
    [Required, StringLength(Limits.NameMax)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(Limits.EmailMax)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    // The seeded administrators log in with their user name ("super"); app users use their email.
    [Required, StringLength(Limits.EmailMax)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public record TokenResponse(string Token);

// MunicipalityId is set for a Municipality Admin (their tenant) and empty for everybody else.
public record UserResponse(string Name, string Email, IReadOnlyList<string> Roles, int? MunicipalityId);
