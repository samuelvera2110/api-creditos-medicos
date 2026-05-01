using HealthCare.Application.Modules.Auth.Security.Interfaces;
using HealthCare.Application.Modules.Auth.Services;
using HealthCare.Domain.Modules.Users;
using HealthCare.Infrastructure.Persistence.Context;
using HealthCare.Infrastructure.Repositories;
using HealthCare.Infrastructure.Security;
using HealthCare.Shared.Constants;
using HeathCare.Api.Middlewares;
using Microsoft.EntityFrameworkCore;

namespace HeathCare.Api.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        
        services.AddSecurity(configuration);
        
        services.AddRepositories();
        
        services.AddApplicationServices();

        services.AddScalar();
        services.AddControllers();
    }
    
    
    
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING_DATABASE")
                               ?? configuration[ConfigurationConstants.CONNECTION_STRING_DATABASE]
                               ?? throw new Exception($"No se encontró la configuración: {ConfigurationConstants.CONNECTION_STRING_DATABASE}");

        services.AddDbContext<HeathCareDbContext>(options =>
            options.UseNpgsql(connectionString));
    }
    
    private static void AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();
    }
  
    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
    }
    
    private static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
    }
    public static void AddScalar(this IServiceCollection services)
    {
        services.AddOpenApi(); 
    }
    
    
}