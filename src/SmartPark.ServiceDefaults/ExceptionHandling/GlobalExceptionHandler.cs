using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        var problemDetails = new ProblemDetails
        {
            Status = mapped.StatusCode,
            Title = mapped.Title,
            Detail = mapped.Message,
            Instance = httpContext.Request.Path,
            Type = $"https://httpstatuses.com/{mapped.StatusCode}"
        };

        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["code"] = mapped.Code;
        problemDetails.Extensions["message"] = mapped.Message;
        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

        if (mapped.Details is not null)
        {
            problemDetails.Extensions["details"] = mapped.Details;
        }

        httpContext.Response.StatusCode = mapped.StatusCode;

        var handled = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });

        if (!handled)
        {
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        }

        return true;
    }
}
