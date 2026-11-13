using MEMS.Data.Entities;
using MEMS.Shared.Dtos;

namespace MEMS.Api.Extensions;

// Entities -> JSON. Logos and photos are stored as file names; the API turns them into absolute URLs
// (files live below /images/logos and /images/candidates).
public static class DtoMapping
{
    public static MunicipalityDto ToDto(this Municipality m, HttpRequest request) => new(
        m.MunicipalityId, m.Name, m.Slug, m.Province, m.ElectionDate, m.Description,
        ImageUrl(request, "logos", m.LogoFileName), m.ThemeColor, m.WelcomeMessage);

    public static PositionDto ToDto(this Position p) => new(
        p.PositionId, p.Name, p.SeatsAvailable, p.MunicipalityId, p.Municipality!.Name);

    public static CandidateDto ToDto(this Candidate c, HttpRequest request) => new(
        c.CandidateId, c.Name, c.Profile, ImageUrl(request, "candidates", c.PhotoFileName), c.Website, c.Email,
        c.PositionId, c.Position!.Name, c.MunicipalityId, c.Municipality!.Name, c.CreatedDate);

    private static string? ImageUrl(HttpRequest request, string folder, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }
        return $"{request.Scheme}://{request.Host}/images/{folder}/{Uri.EscapeDataString(fileName)}";
    }
}
