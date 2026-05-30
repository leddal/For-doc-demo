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
        var (statusCode, root) = await HandleAsync(
            new DomainException("工单状态冲突", "work_order_invalid_status_transition", 409),
            "/api/work-orders/test-id/arrive");

        Assert.Equal(409, statusCode);
        Assert.Equal(409, root.GetProperty("status").GetInt32());
        Assert.Equal("work_order_invalid_status_transition", root.GetProperty("code").GetString());
        Assert.Equal("工单状态冲突", root.GetProperty("message").GetString());
        Assert.True(root.TryGetProperty("traceId", out var traceId));
        Assert.False(string.IsNullOrWhiteSpace(traceId.GetString()));
    }

    [Fact]
    public async Task Should_Write_Standard_ProblemDetails_For_AuthenticationFailedException()
    {
        var (statusCode, root) = await HandleAsync(new AuthenticationFailedException(), "/api/auth/login");

        Assert.Equal(401, statusCode);
        Assert.Equal(401, root.GetProperty("status").GetInt32());
        Assert.Equal("authentication_failed", root.GetProperty("code").GetString());
        Assert.Equal("用户名或密码错误。", root.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Should_Write_Standard_ProblemDetails_For_NotFoundException()
    {
        var (statusCode, root) = await HandleAsync(new NotFoundException("未找到工单。"), "/api/work-orders/missing-id");

        Assert.Equal(404, statusCode);
        Assert.Equal(404, root.GetProperty("status").GetInt32());
        Assert.Equal("resource_not_found", root.GetProperty("code").GetString());
        Assert.Equal("未找到工单。", root.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Should_Write_Standard_ProblemDetails_For_ValidationException()
    {
        var (statusCode, root) = await HandleAsync(
            new ValidationException([
                new ValidationError("userName", "用户名不能为空。"),
                new ValidationError("password", "密码不能为空。")]),
            "/api/auth/login");

        Assert.Equal(422, statusCode);
        Assert.Equal(422, root.GetProperty("status").GetInt32());
        Assert.Equal("validation_failed", root.GetProperty("code").GetString());
        Assert.Equal("请求校验失败。", root.GetProperty("message").GetString());
        Assert.Equal("用户名不能为空。", root.GetProperty("details").GetProperty("userName")[0].GetString());
        Assert.Equal("密码不能为空。", root.GetProperty("details").GetProperty("password")[0].GetString());
    }

    private static async Task<(int StatusCode, JsonElement Root)> HandleAsync(Exception exception, string path)
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
        httpContext.Request.Path = path;
        httpContext.Request.Headers.Accept = "application/problem+json";
        httpContext.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);

        httpContext.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(httpContext.Response.Body);
        return (httpContext.Response.StatusCode, document.RootElement.Clone());
    }
}
