using System.ComponentModel.DataAnnotations;
using MunicipalElections.Web.Models;

namespace MunicipalElections.Web.Dtos;

public class SaveCandidateRequest
{
    [Range(1, int.MaxValue)]
    public int CandidateId { get; set; }
}

public record SavedCandidateDto(int SavedCandidateId, DateTime DateSaved, CandidateDto Candidate)
{
    public static SavedCandidateDto From(SavedCandidate s, HttpRequest request) => new(
        s.SavedCandidateId,
        DateTime.SpecifyKind(s.DateSaved, DateTimeKind.Utc),
        CandidateDto.From(s.Candidate!, request));
}
