namespace MEMS.Data.Entities;

public class Position
{
    public int PositionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SeatsAvailable { get; set; }

    public int MunicipalityId { get; set; }
    public Municipality? Municipality { get; set; }

    public ICollection<Candidate> Candidates { get; set; } = [];
}
