using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

namespace SmartPark.ServiceDefaults.ExceptionHandling;

/// <summary>
/// 全局异常处理器。
/// 负责记录日志、生成统一 ProblemDetails，并补充 traceId / code / message 等扩展字段。
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var mapped = ExceptionMapping.Map(exception);

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        logger.Log(
            mapped.LogLevel,
            exception,
            "请求处理失败。TraceId: {TraceId}; Path: {Path}; ErrorCode: {ErrorCode}",
            traceId,
            httpContext.Request.Path,
            mapped.Code);

        await ProblemDetailsResponseWriter.WriteAsync(httpContext, mapped, problemDetailsService, exception, cancellationToken);
        return true;
    }
}
