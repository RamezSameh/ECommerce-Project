namespace ECommerce.Infrastructure.Services;

/// <summary>JWT + refresh token settings bound from appsettings / environment.</summary>
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int DurationInMinutes { get; set; } = 60;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}