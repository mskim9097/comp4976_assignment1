using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MunicipalElections.Web.Models;

public class CandidateDirectoryViewModel
{
    [StringLength(100)]
    [Display(Name = "Search candidates")]
    public string? Search { get; set; }

    [Display(Name = "Municipality")]
    public int? MunicipalityId { get; set; }

    [Display(Name = "Position")]
    public int? PositionId { get; set; }

    public List<SelectListItem> Municipalities { get; set; } = [];
    public List<SelectListItem> Positions { get; set; } = [];
    // Every position, for the script that updates the Position list when the Municipality changes.
    public List<DirectoryPositionOption> AllPositions { get; set; } = [];
    public List<Candidate> Candidates { get; set; } = [];
    public Dictionary<string, string> MunicipalityElectionDates { get; set; } = [];
}

public class DirectoryPositionOption
{
    public int PositionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MunicipalityId { get; set; }
    public string MunicipalityName { get; set; } = string.Empty;
}
