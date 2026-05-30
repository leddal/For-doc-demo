using Microsoft.Extensions.DependencyInjection;
using SmartPark.SharedKernel;

namespace SmartPark.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IRequestValidator<LoginRequest>, LoginRequestValidator>();
        return services;
    }
}
