using Microsoft.AspNetCore.Http;

namespace SmartPark.ServiceDefaults.ExceptionHandling;

internal static class AuthorizationProblemDetailsResultHandler
{
    public static bool ShouldWrite(HttpContext httpContext)
    {
        return !httpContext.Response.HasStarted
            && string.IsNullOrWhiteSpace(httpContext.Response.ContentType)
            && httpContext.Response.StatusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden;
    }
}
