using Microsoft.Extensions.DependencyInjection;

namespace SmartPark.Collaboration.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCollaborationApplication(this IServiceCollection services)
    {
        services.AddScoped<ICollaborationService, CollaborationService>();
        return services;
    }
}
