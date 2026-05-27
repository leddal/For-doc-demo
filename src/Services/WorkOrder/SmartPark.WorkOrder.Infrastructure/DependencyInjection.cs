using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartPark.SharedInfrastructure.Caching;
using SmartPark.WorkOrder.Application;

namespace SmartPark.WorkOrder.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkOrderInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure(configuration);
        services.AddDbContext<WorkOrderDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
        services.AddScoped<WorkOrderDatabaseInitializer>();
        return services;
    }

    public static async Task UseWorkOrderInfrastructureAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<WorkOrderDatabaseInitializer>();
        await initializer.InitializeAsync();
    }
}
