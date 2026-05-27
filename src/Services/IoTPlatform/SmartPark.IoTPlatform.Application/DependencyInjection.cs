using Microsoft.Extensions.DependencyInjection;

namespace SmartPark.IoTPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIoTPlatformApplication(this IServiceCollection services)
    {
        services.AddScoped<IIoTPlatformService, IoTPlatformService>();
        return services;
    }
}
