using Microsoft.AspNetCore.Authorization;
using SmartPark.IoTPlatform.Application;
using SmartPark.IoTPlatform.Domain;

namespace SmartPark.IoTPlatform.Api;

/// <summary>
/// 物联服务端点。
/// 对外提供监测点、告警、总览和灌溉控制接口。
/// </summary>
public static class IoTEndpoints
{
    public static IEndpointRouteBuilder MapIoTEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var monitoring = endpoints.MapGroup("/api/monitoring-points").RequireAuthorization();
        monitoring.MapGet(string.Empty, QueryMonitoringPointsAsync);
        monitoring.MapPost(string.Empty, CreateMonitoringPointAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Dispatcher" });

        var alerts = endpoints.MapGroup("/api/alerts").RequireAuthorization();
        alerts.MapGet(string.Empty, QueryAlertsAsync);
        alerts.MapPost(string.Empty, CreateAlertAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Dispatcher" });

        var overview = endpoints.MapGroup("/api/overview").RequireAuthorization();
        overview.MapGet("/dashboard", GetOverviewAsync);

        var controls = endpoints.MapGroup("/api/controls").RequireAuthorization();
        controls.MapPost("/irrigation", SendIrrigationCommandAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Operator" });

        return endpoints;
    }

    private static async Task<IResult> QueryMonitoringPointsAsync(string? keyword, MonitoringMetricType? metricType, int pageNumber, int pageSize, IIoTPlatformService service, CancellationToken cancellationToken)
        => Results.Ok(await service.QueryMonitoringPointsAsync(new MonitoringPointQuery(keyword, metricType, pageNumber == 0 ? 1 : pageNumber, pageSize == 0 ? 20 : pageSize), cancellationToken));

    private static async Task<IResult> CreateMonitoringPointAsync(CreateMonitoringPointRequest request, IIoTPlatformService service, CancellationToken cancellationToken)
        => Results.Ok(await service.CreateMonitoringPointAsync(request, cancellationToken));

    private static async Task<IResult> QueryAlertsAsync(AlertLevel? level, AlertStatus? status, int pageNumber, int pageSize, IIoTPlatformService service, CancellationToken cancellationToken)
        => Results.Ok(await service.QueryAlertsAsync(new AlertQuery(level, status, pageNumber == 0 ? 1 : pageNumber, pageSize == 0 ? 20 : pageSize), cancellationToken));

    private static async Task<IResult> CreateAlertAsync(CreateAlertRequest request, IIoTPlatformService service, CancellationToken cancellationToken)
        => Results.Ok(await service.CreateAlertAsync(request, cancellationToken));

    private static async Task<IResult> GetOverviewAsync(IIoTPlatformService service, CancellationToken cancellationToken)
        => Results.Ok(await service.GetOverviewAsync(cancellationToken));

    private static async Task<IResult> SendIrrigationCommandAsync(IrrigationCommandRequest request, IIoTPlatformService service, CancellationToken cancellationToken)
        => Results.Ok(await service.SendIrrigationCommandAsync(request, cancellationToken));
}
