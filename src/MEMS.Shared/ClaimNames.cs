namespace MEMS.Shared;

// Claim names used in the JWT (Api) and in the Admin login cookie.
// A Municipality Admin carries the id of their tenant in "municipalityId".
public static class ClaimNames
{
    public const string Name = "name";
    public const string Role = "role";
    public const string MunicipalityId = "municipalityId";
}
