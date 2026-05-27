using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartPark.Collaboration.Application;
using SmartPark.SharedContracts.Auth;
using SmartPark.SharedInfrastructure.Caching;

namespace SmartPark.Collaboration.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCollaborationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure(configuration);
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddDbContext<CollaborationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddScoped<ICollaborationRepository, CollaborationRepository>();
        services.AddScoped<IInternalTokenProvider, InternalTokenProvider>();
        services.AddHttpClient<IWorkOrderGateway, WorkOrderGateway>(client =>
            client.BaseAddress = new Uri(configuration["Services:WorkOrderBaseUrl"] ?? "http://localhost:5104"));
        services.AddScoped<CollaborationDatabaseInitializer>();
        return services;
    }

    public static async Task UseCollaborationInfrastructureAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<CollaborationDatabaseInitializer>();
        await initializer.InitializeAsync();
    }
}
