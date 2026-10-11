namespace MunicipalElections.Web.Models;

// A candidate in a logged-in user's voting guide (used by the iOS app through the Web API).
public class SavedCandidate
{
    public int SavedCandidateId { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int CandidateId { get; set; }
    public Candidate? Candidate { get; set; }

    public DateTime DateSaved { get; set; }
}
