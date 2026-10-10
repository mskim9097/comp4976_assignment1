using System.ComponentModel.DataAnnotations;

namespace MunicipalElections.Api.Models;

public class Candidate
{
    public int CandidateId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Biography { get; set; }

    [StringLength(500)]
    public string? PhotoUrl { get; set; }

    [StringLength(500)]
    public string? Website { get; set; }

    [StringLength(200)]
    public string? Email { get; set; }

    public int PositionId { get; set; }
    public Position? Position { get; set; }

    public int MunicipalityId { get; set; }
    public Municipality? Municipality { get; set; }
}
