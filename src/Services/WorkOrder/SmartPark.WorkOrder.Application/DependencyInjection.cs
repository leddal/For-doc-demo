using Microsoft.Extensions.DependencyInjection;

namespace SmartPark.WorkOrder.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkOrderApplication(this IServiceCollection services)
    {
        services.AddScoped<IWorkOrderService, WorkOrderService>();
        return services;
    }
}
