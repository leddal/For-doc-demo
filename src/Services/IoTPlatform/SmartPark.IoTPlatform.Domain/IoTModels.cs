using SmartPark.SharedKernel;

namespace SmartPark.IoTPlatform.Domain;

/// <summary>
/// 监测指标类型。
/// 当前按土壤墒情、水位、水质、噪声、气象五类做示例建模。
/// </summary>
public enum MonitoringMetricType
{
    SoilMoisture = 1,
    WaterLevel = 2,
    WaterQuality = 3,
    Noise = 4,
    Weather = 5
}

/// <summary>
/// 告警等级。
/// </summary>
public enum AlertLevel
{
    Info = 1,
    Warning = 2,
    Critical = 3
}

/// <summary>
/// 告警处理状态。
/// </summary>
public enum AlertStatus
{
    Open = 1,
    Acknowledged = 2,
    Resolved = 3
}

/// <summary>
/// 监测点实体。
/// 表示一个具体的物联监测来源或传感器点位。
/// </summary>
public sealed class MonitoringPoint : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public MonitoringMetricType MetricType { get; set; }

    public string Unit { get; set; } = string.Empty;

    public decimal CurrentValue { get; set; }

    public string Area { get; set; } = string.Empty;

    public string StatusText { get; set; } = "Normal";
}

/// <summary>
/// 告警记录实体。
/// 后续可继续扩展为自动转事件、自动转工单的触发源。
/// </summary>
public sealed class AlertRecord : AuditableEntity
{
    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public AlertLevel Level { get; set; }

    public AlertStatus Status { get; set; } = AlertStatus.Open;

    public Guid? RelatedAssetId { get; set; }

    public Guid? MonitoringPointId { get; set; }
}
