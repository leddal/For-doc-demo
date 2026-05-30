using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace SmartPark.ServiceDefaults.ExceptionHandling;

internal static class ProblemDetailsResponseWriter
{
    public static async ValueTask WriteAsync(
        HttpContext httpContext,
        ExceptionDescriptor descriptor,
        IProblemDetailsService problemDetailsService,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        var problemDetails = new ProblemDetails
        {
            Status = descriptor.StatusCode,
            Title = descriptor.Title,
            Detail = descriptor.Message,
            Instance = httpContext.Request.Path,
            Type = $"https://httpstatuses.com/{descriptor.StatusCode}"
        };

        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["code"] = descriptor.Code;
        problemDetails.Extensions["message"] = descriptor.Message;
        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

        if (descriptor.Details is not null)
        {
            problemDetails.Extensions["details"] = descriptor.Details;
        }

        httpContext.Response.StatusCode = descriptor.StatusCode;

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
    }
}
