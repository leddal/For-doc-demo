using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartPark.SharedInfrastructure.Abstractions;
using SmartPark.SharedInfrastructure.Runtime;

namespace SmartPark.SharedInfrastructure.Caching;

public static class CacheServiceCollectionExtensions
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUserAccessor>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);
            services.AddScoped<ICacheService, RedisCacheService>();
        }

        return services;
    }
}
