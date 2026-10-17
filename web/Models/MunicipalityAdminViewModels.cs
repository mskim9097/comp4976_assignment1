using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MunicipalElections.Web.Models;

public class MunicipalityAdminDashboardViewModel
{
    public List<ManagedMunicipalityViewModel> Municipalities { get; set; } = [];
}

public class ManagedMunicipalityViewModel
{
    public Municipality Municipality { get; set; } = null!;
    public List<ManagedPositionViewModel> Positions { get; set; } = [];
}

public class ManagedPositionViewModel
{
    public Position Position { get; set; } = null!;
    public List<Candidate> Candidates { get; set; } = [];
}

public class MunicipalityPositionFormViewModel
{
    public int PositionId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Select a municipality.")]
    [Display(Name = "Municipality")]
    public int MunicipalityId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 100)]
    [Display(Name = "Seats available")]
    public int SeatsAvailable { get; set; } = 1;
}

public class MunicipalityCandidateFormViewModel
{
    public int CandidateId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Select a position.")]
    [Display(Name = "Position")]
    public int PositionId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    [Display(Name = "Biography / Profile")]
    public string? Profile { get; set; }

    [Url, StringLength(500)]
    [Display(Name = "Website URL")]
    public string? Website { get; set; }

    [EmailAddress, StringLength(200)]
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    [Display(Name = "Candidate photo")]
    public IFormFile? PhotoUpload { get; set; }

    [BindNever]
    public string? CurrentPhotoFileName { get; set; }
}
