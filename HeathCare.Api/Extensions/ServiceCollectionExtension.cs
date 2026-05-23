using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Modules.Auth.Security.Interfaces;
using HealthCare.Application.Modules.Auth.Services;
using HealthCare.Application.Modules.Auth.Validators;
using HealthCare.Infrastructure.Email;
using HealthCare.Infrastructure.Persistence.Context;
using HealthCare.Infrastructure.Repositories;
using HealthCare.Infrastructure.Security;
using HealthCare.Shared.Constants;
using HealthCare.Shared.Wrappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HeathCare.Api.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddSecurity(configuration);
        services.AddEmail(configuration);
        services.AddCorsPolicy(configuration);
        services.AddRepositories();
        services.AddApplicationServices();
        services.AddScalar();
        services.AddControllers();
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        services.ConfigureApiBehavior();
    }

    private static void ConfigureApiBehavior(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .SelectMany(e => e.Value!.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return new BadRequestObjectResult(
                    ApiResponse<object>.Error("Errores de validación.", errors));
            };
        });
    }

    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("CONNECTION_STRING_DATABASE")
            ?? configuration[ConfigurationConstants.CONNECTION_STRING_DATABASE]
            ?? throw new Exception(
                $"No se encontró la configuración: {ConfigurationConstants.CONNECTION_STRING_DATABASE}");

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddDbContext<HeathCareDbContext>(options =>
            options.UseNpgsql(connectionString));
    }

    private static void AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddScoped<IJwtProvider, JwtProvider>();

        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                "La sección 'Jwt' no está configurada en appsettings.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(
                                                Encoding.UTF8.GetBytes(jwtSettings.PrivateKey)),
                    ValidateIssuer   = true,
                    ValidIssuer      = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience    = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew        = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
    }

    private static void AddEmail(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpSettings>(options =>
        {
            options.Host = configuration[ConfigurationConstants.SMTP_HOST]
                ?? throw new InvalidOperationException(
                    $"No se encontró la configuración: {ConfigurationConstants.SMTP_HOST}");

            options.User = configuration[ConfigurationConstants.SMTP_USER]
                ?? throw new InvalidOperationException(
                    $"No se encontró la configuración: {ConfigurationConstants.SMTP_USER}");

            options.Password = configuration[ConfigurationConstants.SMTP_PASSWORD]
                ?? throw new InvalidOperationException(
                    $"No se encontró la configuración: {ConfigurationConstants.SMTP_PASSWORD}");

            options.From = configuration[ConfigurationConstants.SMTP_FROM]
                ?? throw new InvalidOperationException(
                    $"No se encontró la configuración: {ConfigurationConstants.SMTP_FROM}");

            options.Port = int.TryParse(
                configuration[ConfigurationConstants.SMTP_PORT], out var port) ? port : 587;

            options.EnableSsl = !bool.TryParse(
                configuration[ConfigurationConstants.SMTP_ENABLE_SSL], out var ssl) || ssl;
        });

        services.AddScoped<IEmailService, EmailService>();
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<UserRepository>()
            .AddClasses(classes => classes
                .InNamespaces("HealthCare.Infrastructure.Repositories"))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    }

    private static void AddApplicationServices(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<AuthService>()
            .AddClasses(classes => classes
                .Where(t => t.Name.EndsWith("Service")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddScoped<IPasswordHasher, PasswordHasher>();
    }

    private static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                if (allowedOrigins.Length > 0)
                    policy.WithOrigins(allowedOrigins);
                else
                    policy.SetIsOriginAllowed(_ => false);

                policy
                    .WithHeaders("Content-Type", "Authorization")
                    .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE");
            });
        });
    }

    public static void AddScalar(this IServiceCollection services)
    {
        services.AddOpenApi();
    }
}
