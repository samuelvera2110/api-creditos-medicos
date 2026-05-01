using HealthCare.Infrastructure.Persistence.Context;
using HealthCare.Shared.Constants;
using HeathCare.Api.Middlewares;
using Microsoft.EntityFrameworkCore;

namespace HeathCare.Api.Extensions;

public static class ServiceCollectionExtension
{
    public static async Task AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);
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
    
  
    
    public static void AddScalar(this IServiceCollection services)
    {
        services.AddOpenApi(); 
    }
    
    
}