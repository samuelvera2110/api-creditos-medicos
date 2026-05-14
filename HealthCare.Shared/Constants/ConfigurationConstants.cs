namespace HealthCare.Shared.Constants;

public static class ConfigurationConstants
{
    public const string FIRST_APP_TIME_USER_FULLNAME = "FirstAppTime:User:FullName";
    public const string FIRST_APP_TIME_USER_EMAIL = "FirstAppTime:User:Email";
    public const string FIRST_APP_TIME_USER_PASSWORD = "FirstAppTime:User:Password";
    public const string FIRST_APP_TIME_USER_POSITION = "FirstAppTime:User:Position";

    // Connection strings
    public const string CONNECTION_STRING_DATABASE = "ConnectionStrings:DefaultConnection";

    // JWT
    public const string JWT_PRIVATE_KEY = "Jwt:PrivateKey";
    public const string JWT_AUDIENCE = "Jwt:Audience";
    public const string JWT_ISSUER = "Jwt:Issuer";
    public const string JWT_EXPIRATION_MINUTES = "Jwt:ExpirationMinutes";
    public const string JWT_EXPIRATION_IN_MINUTES_MAX = "Jwt:ExpirationInMinutesMax";

    // Auth
    public const string AUTH_REFRESH_TOKEN_EXPIRATION_IN_DAYS = "Auth:RefreshToken:ExpirationInDays";

    // SMTP
    public const string SMTP_HOST = "SMTP:Host";
    public const string SMTP_PORT = "SMTP:Port";
    public const string SMTP_USER = "SMTP:User";
    public const string SMTP_PASSWORD = "SMTP:Password";
    public const string SMTP_FROM = "SMTP:From";
    public const string SMTP_ENABLE_SSL = "SMTP:EnableSsl";

    // App
    public const string VERSION = "Version";

    
}