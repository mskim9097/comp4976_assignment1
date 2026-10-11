using MunicipalElections.Web.Extensions;
using MunicipalElections.Web.Models;

namespace MunicipalElections.Web.Dtos;

// JSON shapes returned by the Web API (used by the iOS app). They are built from the shared entities:
// Profile -> biography, PhotoFileName/LogoFileName -> absolute URLs, municipality taken from the position.

// ElectionDate is sent as a date-only value ("2026-10-17") so it does not shift with time zones.
public record MunicipalityDto(
    int MunicipalityId,
    string Name,
    string Province,
    DateOnly ElectionDate,
    string? Description,
    string? LogoUrl)
{
    public static MunicipalityDto From(Municipality m, HttpRequest request) => new(
        m.MunicipalityId,
        m.Name,
        m.Province,
        DateOnly.FromDateTime(m.ElectionDate),
        m.Description,
        request.LogoUrl(m.LogoFileName));
}

public record PositionDto(
    int PositionId,
    string Name,
    int SeatsAvailable,
    int MunicipalityId,
    string MunicipalityName)
{
    public static PositionDto From(Position p) => new(
        p.PositionId,
        p.Name,
        p.SeatsAvailable,
        p.MunicipalityId,
        p.Municipality!.Name);
}

public record CandidateDto(
    int CandidateId,
    string Name,
    string? Biography,
    string? PhotoUrl,
    string? Website,
    string? Email,
    int PositionId,
    string PositionName,
    int MunicipalityId,
    string MunicipalityName)
{
    public static CandidateDto From(Candidate c, HttpRequest request) => new(
        c.CandidateId,
        c.Name,
        c.Profile,
        request.PhotoUrl(c.PhotoFileName),
        c.Website,
        c.Email,
        c.PositionId,
        c.Position!.Name,
        c.Position.MunicipalityId,
        c.Position.Municipality!.Name);
}
