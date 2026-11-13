using System.Text;
using MEMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace MEMS.Api.Auth;

// Claim names used in the token. A Municipality Admin carries the id of their tenant.
public static class ClaimNames
{
    public const string Name = "name";
    public const string Role = "role";
    public const string MunicipalityId = "municipalityId";
}

public class JwtTokenService(JwtSettings settings, UserManager<ApplicationUser> users)
{
    public async Task<string> CreateTokenAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimNames.Name, user.Name)
        };
        claims.AddRange((await users.GetRolesAsync(user)).Select(role => new Claim(ClaimNames.Role, role)));
        if (user.MunicipalityId is int tenantId)
        {
            claims.Add(new Claim(ClaimNames.MunicipalityId, tenantId.ToString()));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            Expires = DateTime.UtcNow.AddMinutes(settings.ExpiresMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)), SecurityAlgorithms.HmacSha256)
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
