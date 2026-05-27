using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartPark.IoTPlatform.Application;
using SmartPark.SharedInfrastructure.Caching;

namespace SmartPark.IoTPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIoTPlatformInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure(configuration);
        services.AddDbContext<IoTPlatformDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddScoped<IIoTRepository, IoTRepository>();
        services.AddScoped<IoTDatabaseInitializer>();
        return services;
    }

    public static async Task UseIoTPlatformInfrastructureAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IoTDatabaseInitializer>();
        await initializer.InitializeAsync();
    }
}
