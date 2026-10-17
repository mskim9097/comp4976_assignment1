using System.ComponentModel.DataAnnotations;

namespace MunicipalElections.Web.Models;

public class MunicipalityFormViewModel
{
    public int MunicipalityId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Province { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Election Date")]
    public DateTime ElectionDate { get; set; } = DateTime.Today;

    [StringLength(2000)]
    public string? Description { get; set; }
}

public class MunicipalityAdminViewModel
{
    public Municipality Municipality { get; set; } = null!;
    public List<ApplicationUser> Admins { get; set; } = [];
    public int PositionCount { get; set; }
}

public class SuperAdminIndexViewModel
{
    public List<MunicipalityAdminViewModel> Municipalities { get; set; } = [];
}

public class CreateMunicipalityAdminViewModel
{
    [Required]
    [Display(Name = "Municipality")]
    public int MunicipalityId { get; set; }

    [Required, StringLength(256)]
    [Display(Name = "User name")]
    public string UserName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Full name")]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
