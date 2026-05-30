using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
            app.UseStatusCodePages(async context =>
            {
                var httpContext = context.HttpContext;
                if (!AuthorizationProblemDetailsResultHandler.ShouldWrite(httpContext))
                {
                    return;
                }

                var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
                var descriptor = ExceptionMapping.MapStatusCode(httpContext.Response.StatusCode);
                await ProblemDetailsResponseWriter.WriteAsync(httpContext, descriptor, problemDetailsService, null, httpContext.RequestAborted);
            });

            return app;
        }
    }
}
