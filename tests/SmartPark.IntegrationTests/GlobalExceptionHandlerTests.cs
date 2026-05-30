using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartPark.ServiceDefaults.ExceptionHandling;
using SmartPark.SharedKernel;

namespace SmartPark.IntegrationTests;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task Should_Write_Standard_ProblemDetails_For_DomainException()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();

        using var serviceProvider = services.BuildServiceProvider();
        var handler = new GlobalExceptionHandler(
            serviceProvider.GetRequiredService<ILogger<GlobalExceptionHandler>>(),
            serviceProvider.GetRequiredService<IProblemDetailsService>());

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };
        httpContext.Request.Path = "/api/work-orders/test-id/arrive";
        httpContext.Request.Headers.Accept = "application/problem+json";
        httpContext.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(
            httpContext,
            new DomainException("工单状态冲突", "work_order_invalid_status_transition", 409),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(409, httpContext.Response.StatusCode);

        httpContext.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(httpContext.Response.Body);
        var root = document.RootElement;

        Assert.Equal(409, root.GetProperty("status").GetInt32());
        Assert.Equal("work_order_invalid_status_transition", root.GetProperty("code").GetString());
        Assert.Equal("工单状态冲突", root.GetProperty("message").GetString());
        Assert.True(root.TryGetProperty("traceId", out var traceId));
        Assert.False(string.IsNullOrWhiteSpace(traceId.GetString()));
    }
}
