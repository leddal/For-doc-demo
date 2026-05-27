using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartPark.Asset.Application;
using SmartPark.SharedInfrastructure.Caching;

namespace SmartPark.Asset.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAssetInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure(configuration);
        services.AddDbContext<AssetDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<AssetDatabaseInitializer>();
        return services;
    }

    public static async Task UseAssetInfrastructureAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<AssetDatabaseInitializer>();
        await initializer.InitializeAsync();
    }
}
