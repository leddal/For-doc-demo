using SmartPark.SharedKernel;

namespace SmartPark.Collaboration.Domain;

/// <summary>
/// 事件严重程度。
/// </summary>
public enum EventSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// 事件状态。
/// 当前最小闭环为：待处置、已转工单、已关闭。
/// </summary>
public enum EventStatus
{
    Open = 1,
    WorkOrderCreated = 2,
    Closed = 3
}

/// <summary>
/// 事件台账聚合根。
/// 作为“物联预警/人工上报”与“工单处置”的中间协同对象。
/// </summary>
public sealed class EventRecord : AuditableEntity
{
    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public EventSeverity Severity { get; set; }

    public EventStatus Status { get; private set; } = EventStatus.Open;

    public string Area { get; set; } = string.Empty;

    public Guid? RelatedAssetId { get; set; }

    public Guid? RelatedAlertId { get; set; }

    public Guid? WorkOrderId { get; private set; }

    public string? WorkOrderNumber { get; private set; }

    public DateTimeOffset? ClosedAt { get; private set; }

    public string? ClosedNote { get; private set; }

    /// <summary>
    /// 事件转工单后，回写工单标识，形成事件到工单的追踪链路。
    /// </summary>
    public void BindWorkOrder(Guid workOrderId, string workOrderNumber)
    {
        WorkOrderId = workOrderId;
        WorkOrderNumber = workOrderNumber;
        Status = EventStatus.WorkOrderCreated;
    }

    public void Close(string? note, DateTimeOffset now)
    {
        Status = EventStatus.Closed;
        ClosedNote = note;
        ClosedAt = now;
    }
}

/// <summary>
/// 公告实体。
/// 当前由协同服务统一维护，供前端首页和公共服务模块展示。
/// </summary>
public sealed class Announcement : AuditableEntity
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public bool IsPublished { get; set; } = true;

    public DateTimeOffset PublishedAt { get; set; } = DateTimeOffset.UtcNow;
}
