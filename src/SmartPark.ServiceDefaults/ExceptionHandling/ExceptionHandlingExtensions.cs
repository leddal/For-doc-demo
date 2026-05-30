using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SmartPark.ServiceDefaults.ExceptionHandling;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 注册 SmartPark 微服务统一异常处理能力。
    /// </summary>
    public static class ExceptionHandlingServiceCollectionExtensions
    {
        public static IServiceCollection AddSmartParkExceptionHandling(this IServiceCollection services)
        {
            services.AddProblemDetails();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            return services;
        }
    }
}

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// 挂载 SmartPark 统一异常处理中间件。
    /// </summary>
    public static class ExceptionHandlingApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSmartParkExceptionHandling(this IApplicationBuilder app)
        {
            app.UseExceptionHandler();
            return app;
        }
    }
}
