using Microsoft.Extensions.DependencyInjection;
using SmartPark.SharedKernel;

namespace SmartPark.WorkOrder.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkOrderApplication(this IServiceCollection services)
    {
        services.AddScoped<IWorkOrderService, WorkOrderService>();
        services.AddScoped<IRequestValidator<CreateWorkOrderRequest>, CreateWorkOrderRequestValidator>();
        services.AddScoped<IRequestValidator<DispatchWorkOrderRequest>, DispatchWorkOrderRequestValidator>();
        services.AddScoped<IRequestValidator<ActionRequest>, ActionRequestValidator>();
        return services;
    }
}
