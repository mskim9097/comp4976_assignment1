namespace MEMS.Api.Auth;

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "MEMS.Api";
    public string Audience { get; set; } = "MEMS.Clients";
    public int ExpiresMinutes { get; set; } = 60;
}
