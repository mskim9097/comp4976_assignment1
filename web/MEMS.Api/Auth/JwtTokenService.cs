using System.Security.Claims;
using System.Text;
using MEMS.Data.Entities;
using MEMS.Shared;
using MEMS.Shared.Dtos;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MEMS.Api.Auth;

public class JwtTokenService(JwtSettings settings)
{
    // Claim names used inside the token (MapInboundClaims is off, so they stay exactly like this).
    public const string RoleClaim = "role";
    public const string NameClaim = "name";

    public TokenResponse CreateToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var roleList = roles.ToList();
        var expires = DateTime.UtcNow.AddMinutes(settings.ExpiresMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(NameClaim, user.UserName ?? user.Id)
        };
        claims.AddRange(roleList.Select(r => new Claim(RoleClaim, r)));
        if (user.MunicipalityId is int tenantId)
        {
            claims.Add(new Claim(MemsClaims.MunicipalityId, tenantId.ToString()));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            Expires = expires,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)), SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new TokenResponse(token, expires, user.UserName ?? string.Empty, roleList.FirstOrDefault() ?? string.Empty, user.MunicipalityId);
    }
}
