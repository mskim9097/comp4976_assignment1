using System.ComponentModel.DataAnnotations;

namespace MEMS.Shared.Dtos;

// The numbers on the admin dashboard. A Municipality Admin only counts their own municipality.
public record DashboardCounts(int Municipalities, int Positions, int Candidates);

// A Super Admin creates the administrator accounts of a municipality.
public class CreateAdminRequest
{
    [Required(ErrorMessage = "User name is required."), StringLength(Limits.NameMax)]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required."), StringLength(Limits.NameMax)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Enter a valid email address."), StringLength(Limits.EmailMax)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required."), StringLength(100, MinimumLength = 6, ErrorMessage = "The password needs at least 6 characters.")]
    public string Password { get; set; } = string.Empty;
}
