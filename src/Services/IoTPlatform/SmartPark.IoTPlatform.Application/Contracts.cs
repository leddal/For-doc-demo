using SmartPark.IoTPlatform.Domain;
using SmartPark.SharedContracts.Common;

namespace SmartPark.IoTPlatform.Application;

public sealed record MonitoringPointQuery(string? Keyword, MonitoringMetricType? MetricType, int PageNumber = 1, int PageSize = 20);

public sealed record AlertQuery(AlertLevel? Level, AlertStatus? Status, int PageNumber = 1, int PageSize = 20);

public sealed record CreateMonitoringPointRequest(string Name, MonitoringMetricType MetricType, string Unit, decimal CurrentValue, string Area, string StatusText);

public sealed record CreateAlertRequest(string Title, string Message, AlertLevel Level, Guid? RelatedAssetId, Guid? MonitoringPointId);

public sealed record IrrigationCommandRequest(string Area, string Action, int DurationMinutes);

public sealed record MonitoringPointDto(Guid Id, string Name, MonitoringMetricType MetricType, string Unit, decimal CurrentValue, string Area, string StatusText, DateTimeOffset CreatedAt);

public sealed record AlertDto(Guid Id, string Title, string Message, AlertLevel Level, AlertStatus Status, Guid? RelatedAssetId, Guid? MonitoringPointId, DateTimeOffset CreatedAt);

public sealed record OverviewDto(int MonitoringPointCount, int OpenAlertCount, int CriticalAlertCount, IReadOnlyCollection<AlertDto> RecentAlerts);

public sealed record IrrigationCommandResult(string Area, string Action, int DurationMinutes, string Status);

public interface IIoTRepository
{
    Task AddMonitoringPointAsync(MonitoringPoint entity, CancellationToken cancellationToken = default);

    Task<PagedResult<MonitoringPoint>> QueryMonitoringPointsAsync(MonitoringPointQuery query, CancellationToken cancellationToken = default);

    Task AddAlertAsync(AlertRecord entity, CancellationToken cancellationToken = default);

    Task<PagedResult<AlertRecord>> QueryAlertsAsync(AlertQuery query, CancellationToken cancellationToken = default);

    Task<int> CountMonitoringPointsAsync(CancellationToken cancellationToken = default);

    Task<int> CountAlertsAsync(AlertLevel? level, AlertStatus? status, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AlertRecord>> GetRecentAlertsAsync(int take, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IIoTPlatformService
{
    Task<PagedResult<MonitoringPointDto>> QueryMonitoringPointsAsync(MonitoringPointQuery query, CancellationToken cancellationToken = default);

    Task<MonitoringPointDto> CreateMonitoringPointAsync(CreateMonitoringPointRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<AlertDto>> QueryAlertsAsync(AlertQuery query, CancellationToken cancellationToken = default);

    Task<AlertDto> CreateAlertAsync(CreateAlertRequest request, CancellationToken cancellationToken = default);

    Task<OverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);

    Task<IrrigationCommandResult> SendIrrigationCommandAsync(IrrigationCommandRequest request, CancellationToken cancellationToken = default);
}
