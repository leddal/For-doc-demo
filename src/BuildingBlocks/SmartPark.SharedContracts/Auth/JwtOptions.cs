namespace SmartPark.SharedContracts.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "SmartPark";

    public string Audience { get; set; } = "SmartPark.Client";

    public string SigningKey { get; set; } = "SmartPark.SuperSecret.SigningKey.2025";

    public int ExpiresMinutes { get; set; } = 120;
}
