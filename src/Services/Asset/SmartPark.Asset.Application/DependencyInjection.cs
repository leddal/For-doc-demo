using Microsoft.Extensions.DependencyInjection;

namespace SmartPark.Asset.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAssetApplication(this IServiceCollection services)
    {
        services.AddScoped<IAssetService, AssetService>();
        return services;
    }
}
