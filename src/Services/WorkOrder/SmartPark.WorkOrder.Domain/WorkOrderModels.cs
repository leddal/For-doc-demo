using SmartPark.SharedKernel;

namespace SmartPark.WorkOrder.Domain;

/// <summary>
/// 工单流转状态。
/// 顺序即本项目默认的标准闭环：创建 → 派发 → 接单 → 到场 → 处理中 → 完工 → 核验 → 关闭。
/// </summary>
public enum WorkOrderStatus
{
    Created = 1,
    Dispatched = 2,
    Accepted = 3,
    Arrived = 4,
    InProgress = 5,
    Completed = 6,
    Verified = 7,
    Closed = 8
}

/// <summary>
/// 工单来源类型。
/// </summary>
public enum WorkOrderSourceType
{
    Manual = 1,
    Event = 2,
    Alert = 3
}

/// <summary>
/// 工单业务分类。
/// </summary>
public enum WorkOrderBusinessType
{
    Inspection = 1,
    Maintenance = 2,
    Cleaning = 3,
    Gardening = 4,
    FloodControl = 5
}

/// <summary>
/// 工单优先级。
/// </summary>
public enum WorkOrderPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// 工单聚合根。
/// 负责承载工单核心字段，并在领域层严格约束状态流转规则。
/// </summary>
public sealed class WorkOrder : AuditableEntity
{
    public string Number { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public WorkOrderSourceType SourceType { get; set; }

    public WorkOrderBusinessType BusinessType { get; set; }

    public WorkOrderPriority Priority { get; set; }

    public WorkOrderStatus Status { get; private set; } = WorkOrderStatus.Created;

    public string ParkArea { get; set; } = string.Empty;

    public Guid? RelatedAssetId { get; set; }

    public Guid? RelatedEventId { get; set; }

    public Guid? RelatedAlertId { get; set; }

    public string? ReporterName { get; set; }

    public string? DispatcherUserId { get; private set; }

    public string? DispatcherName { get; private set; }

    public string? AssigneeUserId { get; private set; }

    public string? AssigneeName { get; private set; }

    public string? ReviewerUserId { get; private set; }

    public string? ReviewerName { get; private set; }

    public string? CompletionNote { get; private set; }

    public string? VerificationNote { get; private set; }

    public string? CloseNote { get; private set; }

    public DateTimeOffset? DispatchedAt { get; private set; }

    public DateTimeOffset? AcceptedAt { get; private set; }

    public DateTimeOffset? ArrivedAt { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? VerifiedAt { get; private set; }

    public DateTimeOffset? ClosedAt { get; private set; }

    public ICollection<WorkOrderActionLog> ActionLogs { get; set; } = new List<WorkOrderActionLog>();

    public ICollection<WorkOrderAttachment> Attachments { get; set; } = new List<WorkOrderAttachment>();

    /// <summary>
    /// 创建工单时默认进入 Created 状态，并记录第一条操作日志。
    /// </summary>
    public static WorkOrder Create(
        string number,
        string title,
        string description,
        WorkOrderSourceType sourceType,
        WorkOrderBusinessType businessType,
        WorkOrderPriority priority,
        string parkArea,
        Guid? relatedAssetId,
        Guid? relatedEventId,
        Guid? relatedAlertId,
        string? reporterName,
        string actorId,
        string actorName,
        DateTimeOffset now)
    {
        var workOrder = new WorkOrder
        {
            Number = number,
            Title = title,
            Description = description,
            SourceType = sourceType,
            BusinessType = businessType,
            Priority = priority,
            ParkArea = parkArea,
            RelatedAssetId = relatedAssetId,
            RelatedEventId = relatedEventId,
            RelatedAlertId = relatedAlertId,
            ReporterName = reporterName
        };

        workOrder.AddActionLog("Create", null, WorkOrderStatus.Created, actorId, actorName, title, now);
        return workOrder;
    }

    /// <summary>
    /// 为工单补充现场图片或附件链接。
    /// </summary>
    public void AddAttachment(string fileName, string url, string contentType)
    {
        Attachments.Add(new WorkOrderAttachment
        {
            FileName = fileName,
            Url = url,
            ContentType = contentType
        });
    }

    /// <summary>
    /// 派单只能从 Created 进入 Dispatched。
    /// </summary>
    public void Dispatch(string dispatcherUserId, string dispatcherName, string assigneeUserId, string assigneeName, string? comment, DateTimeOffset now)
    {
        EnsureStatus(WorkOrderStatus.Created);
        DispatcherUserId = dispatcherUserId;
        DispatcherName = dispatcherName;
        AssigneeUserId = assigneeUserId;
        AssigneeName = assigneeName;
        DispatchedAt = now;
        TransitionTo(WorkOrderStatus.Dispatched, "Dispatch", dispatcherUserId, dispatcherName, comment, now);
    }

    public void Accept(string actorId, string actorName, string? comment, DateTimeOffset now)
    {
        EnsureStatus(WorkOrderStatus.Dispatched);
        AcceptedAt = now;
        TransitionTo(WorkOrderStatus.Accepted, "Accept", actorId, actorName, comment, now);
    }

    public void Arrive(string actorId, string actorName, string? comment, DateTimeOffset now)
    {
        EnsureStatus(WorkOrderStatus.Accepted);
        ArrivedAt = now;
        TransitionTo(WorkOrderStatus.Arrived, "Arrive", actorId, actorName, comment, now);
    }

    public void Start(string actorId, string actorName, string? comment, DateTimeOffset now)
    {
        EnsureStatus(WorkOrderStatus.Arrived);
        StartedAt = now;
        TransitionTo(WorkOrderStatus.InProgress, "Start", actorId, actorName, comment, now);
    }

    public void Complete(string actorId, string actorName, string? comment, DateTimeOffset now)
    {
        EnsureStatus(WorkOrderStatus.InProgress);
        CompletedAt = now;
        CompletionNote = comment;
        TransitionTo(WorkOrderStatus.Completed, "Complete", actorId, actorName, comment, now);
    }

    public void Verify(string actorId, string actorName, string? comment, DateTimeOffset now)
    {
        EnsureStatus(WorkOrderStatus.Completed);
        ReviewerUserId = actorId;
        ReviewerName = actorName;
        VerifiedAt = now;
        VerificationNote = comment;
        TransitionTo(WorkOrderStatus.Verified, "Verify", actorId, actorName, comment, now);
    }

    public void Close(string actorId, string actorName, string? comment, DateTimeOffset now)
    {
        EnsureStatus(WorkOrderStatus.Verified);
        ClosedAt = now;
        CloseNote = comment;
        TransitionTo(WorkOrderStatus.Closed, "Close", actorId, actorName, comment, now);
    }

    /// <summary>
    /// 所有流转都必须严格按既定顺序执行，防止跳状态。
    /// </summary>
    private void EnsureStatus(WorkOrderStatus expected)
    {
        if (Status != expected)
        {
            throw new DomainException($"工单当前状态为 {Status}，不能执行期望状态 {expected} 的流转。");
        }
    }

    private void TransitionTo(WorkOrderStatus nextStatus, string action, string actorId, string actorName, string? comment, DateTimeOffset now)
    {
        var previousStatus = Status;
        Status = nextStatus;
        AddActionLog(action, previousStatus, nextStatus, actorId, actorName, comment, now);
    }

    private void AddActionLog(string action, WorkOrderStatus? fromStatus, WorkOrderStatus toStatus, string actorId, string actorName, string? comment, DateTimeOffset occurredAt)
    {
        ActionLogs.Add(new WorkOrderActionLog
        {
            Action = action,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            OperatorUserId = actorId,
            OperatorName = actorName,
            Comment = comment,
            OccurredAt = occurredAt
        });
    }
}

/// <summary>
/// 工单操作日志，用于追踪每一步流转和处理说明。
/// </summary>
public sealed class WorkOrderActionLog : Entity
{
    public Guid WorkOrderId { get; set; }

    public WorkOrder WorkOrder { get; set; } = null!;

    public string Action { get; set; } = string.Empty;

    public WorkOrderStatus? FromStatus { get; set; }

    public WorkOrderStatus ToStatus { get; set; }

    public string OperatorUserId { get; set; } = string.Empty;

    public string OperatorName { get; set; } = string.Empty;

    public string? Comment { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}

/// <summary>
/// 工单附件实体，用于存储图片、文档等外部资源引用。
/// </summary>
public sealed class WorkOrderAttachment : Entity
{
    public Guid WorkOrderId { get; set; }

    public WorkOrder WorkOrder { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;
}
