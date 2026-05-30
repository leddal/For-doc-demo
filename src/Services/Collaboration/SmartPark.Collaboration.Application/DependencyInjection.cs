using Microsoft.Extensions.DependencyInjection;
using SmartPark.SharedKernel;

namespace SmartPark.Collaboration.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCollaborationApplication(this IServiceCollection services)
    {
        services.AddScoped<ICollaborationService, CollaborationService>();
        services.AddScoped<IRequestValidator<CreateEventRequest>, CreateEventRequestValidator>();
        services.AddScoped<IRequestValidator<CloseEventRequest>, CloseEventRequestValidator>();
        services.AddScoped<IRequestValidator<CreateWorkOrderFromEventRequest>, CreateWorkOrderFromEventRequestValidator>();
        services.AddScoped<IRequestValidator<CreateAnnouncementRequest>, CreateAnnouncementRequestValidator>();
        return services;
    }
}
