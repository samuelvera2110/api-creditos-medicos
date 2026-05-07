namespace HealthCare.Infrastructure.Security;

public class JwtSettings
{
    public string PrivateKey { get; set; } = string.Empty; 
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; }
}