namespace MEMS.Shared;

// Rules for a voter's personal guide. A voter belongs to one municipality and may choose at most as many
// candidates for a position as it has seats (one mayor, for example).
public static class SelectionRules
{
    public record Choice(int CandidateId, int PositionId);

    // Returns the reason why the candidate cannot be added, or null when it is allowed.
    public static string? CheckAdd(
        string? guideMunicipality,
        IReadOnlyCollection<Choice> selected,
        string municipality,
        Choice candidate,
        string positionName,
        int seats)
    {
        if (selected.Count > 0 && guideMunicipality is not null &&
            !string.Equals(guideMunicipality, municipality, StringComparison.OrdinalIgnoreCase))
        {
            return $"You can only choose candidates from one municipality ({guideMunicipality}). " +
                   $"Clear your guide first to choose candidates from {municipality}.";
        }

        if (selected.Any(c => c.CandidateId == candidate.CandidateId))
        {
            return "This candidate is already in your guide.";
        }

        if (seats > 0 && selected.Count(c => c.PositionId == candidate.PositionId) >= seats)
        {
            return $"{positionName} in {municipality} has {seats} {(seats == 1 ? "seat" : "seats")}, " +
                   $"so you can choose at most {seats}. Remove a choice first to pick someone else.";
        }

        return null;
    }
}
