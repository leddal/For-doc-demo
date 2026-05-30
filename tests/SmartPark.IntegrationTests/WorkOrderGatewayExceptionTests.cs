using System.Net;
using System.Text;
using SmartPark.Collaboration.Application;
using SmartPark.Collaboration.Domain;
using SmartPark.Collaboration.Infrastructure;
using SmartPark.SharedKernel;

namespace SmartPark.IntegrationTests;

public sealed class WorkOrderGatewayExceptionTests
{
    [Fact]
    public async Task Should_Throw_DomainException_When_Downstream_Returns_Business_Error()
    {
        var gateway = CreateGateway(HttpStatusCode.Conflict, """
        {
          "code": "work_order_invalid_status_transition",
          "message": "工单状态冲突"
        }
        """);

        var exception = await Assert.ThrowsAsync<DomainException>(() => gateway.CreateFromEventAsync(CreateEventRecord(), new CreateWorkOrderFromEventRequest()));

        Assert.Equal(409, exception.StatusCode);
        Assert.Equal("work_order_invalid_status_transition", exception.Code);
        Assert.Equal("工单状态冲突", exception.Message);
    }

    [Fact]
    public async Task Should_Throw_IntegrationException_When_Downstream_Returns_Server_Error()
    {
        var gateway = CreateGateway(HttpStatusCode.InternalServerError, """
        {
          "code": "work_order_service_error",
          "message": "下游服务异常"
        }
        """);

        var exception = await Assert.ThrowsAsync<IntegrationException>(() => gateway.CreateFromEventAsync(CreateEventRecord(), new CreateWorkOrderFromEventRequest()));

        Assert.Equal(502, exception.StatusCode);
        Assert.Equal("work_order_service_error", exception.Code);
        Assert.Equal("下游服务异常", exception.Message);
    }

    private static WorkOrderGateway CreateGateway(HttpStatusCode statusCode, string json)
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler(statusCode, json))
        {
            BaseAddress = new Uri("http://work-order-service")
        };

        return new WorkOrderGateway(httpClient, new FakeTokenProvider());
    }

    private static EventRecord CreateEventRecord()
        => new()
        {
            Code = "EVT-TEST-001",
            Title = "测试事件",
            Description = "测试异常映射",
            Area = "东门片区",
            Severity = EventSeverity.High
        };

    private sealed class FakeTokenProvider : IInternalTokenProvider
    {
        public string CreateAdminToken() => "fake-token";
    }

    private sealed class StubHttpMessageHandler(HttpStatusCode statusCode, string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
    }
}
