using SmartPark.IoTPlatform.Domain;
using SmartPark.SharedContracts.Common;

namespace SmartPark.IoTPlatform.Application;

public sealed class IoTPlatformService(IIoTRepository repository) : IIoTPlatformService
{
    public async Task<PagedResult<MonitoringPointDto>> QueryMonitoringPointsAsync(MonitoringPointQuery query, CancellationToken cancellationToken = default)
    {
        var result = await repository.QueryMonitoringPointsAsync(query, cancellationToken);
        return new PagedResult<MonitoringPointDto>(result.Items.Select(MapPoint).ToArray(), result.TotalCount, result.PageNumber, result.PageSize);
    }

    public async Task<MonitoringPointDto> CreateMonitoringPointAsync(CreateMonitoringPointRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new MonitoringPoint
        {
            Name = request.Name,
            MetricType = request.MetricType,
            Unit = request.Unit,
            CurrentValue = request.CurrentValue,
            Area = request.Area,
            StatusText = request.StatusText
        };

        await repository.AddMonitoringPointAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return MapPoint(entity);
    }

    public async Task<PagedResult<AlertDto>> QueryAlertsAsync(AlertQuery query, CancellationToken cancellationToken = default)
    {
        var result = await repository.QueryAlertsAsync(query, cancellationToken);
        return new PagedResult<AlertDto>(result.Items.Select(MapAlert).ToArray(), result.TotalCount, result.PageNumber, result.PageSize);
    }

    public async Task<AlertDto> CreateAlertAsync(CreateAlertRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new AlertRecord
        {
            Title = request.Title,
            Message = request.Message,
            Level = request.Level,
            RelatedAssetId = request.RelatedAssetId,
            MonitoringPointId = request.MonitoringPointId
        };

        await repository.AddAlertAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return MapAlert(entity);
    }

    public async Task<OverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var monitoringPointCount = await repository.CountMonitoringPointsAsync(cancellationToken);
        var openAlertCount = await repository.CountAlertsAsync(null, AlertStatus.Open, cancellationToken);
        var criticalAlertCount = await repository.CountAlertsAsync(AlertLevel.Critical, AlertStatus.Open, cancellationToken);
        var recentAlerts = (await repository.GetRecentAlertsAsync(5, cancellationToken)).Select(MapAlert).ToArray();
        return new OverviewDto(monitoringPointCount, openAlertCount, criticalAlertCount, recentAlerts);
    }

    public Task<IrrigationCommandResult> SendIrrigationCommandAsync(IrrigationCommandRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(new IrrigationCommandResult(request.Area, request.Action, request.DurationMinutes, "Accepted"));

    private static MonitoringPointDto MapPoint(MonitoringPoint entity)
        => new(entity.Id, entity.Name, entity.MetricType, entity.Unit, entity.CurrentValue, entity.Area, entity.StatusText, entity.CreatedAt);

    private static AlertDto MapAlert(AlertRecord entity)
        => new(entity.Id, entity.Title, entity.Message, entity.Level, entity.Status, entity.RelatedAssetId, entity.MonitoringPointId, entity.CreatedAt);
}
