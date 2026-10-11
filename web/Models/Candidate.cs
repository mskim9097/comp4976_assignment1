using System.ComponentModel.DataAnnotations;

namespace MunicipalElections.Web.Models;

public class Candidate
{
    public int CandidateId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "The profile can be at most 2000 characters.")]
    [Display(Name = "Biography / Profile")]
    public string? Profile { get; set; }

    // Name of the uploaded photo file (see ImageUrls for where it is served from).
    [StringLength(255)]
    [Display(Name = "Photo")]
    public string? PhotoFileName { get; set; }

    [Url, StringLength(500)]
    [Display(Name = "Website URL")]
    public string? Website { get; set; }

    [EmailAddress, StringLength(200)]
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    [Display(Name = "Date Created")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // The municipality is reached through the position, so the two can never disagree.
    [Range(1, int.MaxValue, ErrorMessage = "Position is required.")]
    [Display(Name = "Position")]
    public int PositionId { get; set; }
    public Position? Position { get; set; }
}
