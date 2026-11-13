namespace MEMS.Data.Entities;

public class Candidate
{
    public int CandidateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Profile { get; set; }
    public string? PhotoFileName { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int PositionId { get; set; }
    public Position? Position { get; set; }

    // The tenant. The database keeps it equal to the municipality of the position (composite foreign key).
    public int MunicipalityId { get; set; }
    public Municipality? Municipality { get; set; }
}
