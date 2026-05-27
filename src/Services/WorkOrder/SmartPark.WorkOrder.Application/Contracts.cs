using SmartPark.SharedContracts.Common;
using SmartPark.WorkOrder.Domain;

namespace SmartPark.WorkOrder.Application;

public sealed record ActionContext(string ActorId, string ActorName);

public sealed record WorkOrderAttachmentInput(string FileName, string Url, string ContentType);

public sealed record CreateWorkOrderRequest(
    string Title,
    string Description,
    WorkOrderSourceType SourceType,
    WorkOrderBusinessType BusinessType,
    WorkOrderPriority Priority,
    string ParkArea,
    Guid? RelatedAssetId,
    Guid? RelatedEventId,
    Guid? RelatedAlertId,
    string? ReporterName,
    IReadOnlyCollection<WorkOrderAttachmentInput>? Attachments);

public sealed record DispatchWorkOrderRequest(string AssigneeUserId, string AssigneeName, string? Comment);

public sealed record ActionRequest(string? Comment);

public sealed record WorkOrderQuery(
    string? Keyword,
    WorkOrderStatus? Status,
    WorkOrderBusinessType? BusinessType,
    WorkOrderPriority? Priority,
    string? AssigneeUserId,
    int PageNumber = 1,
    int PageSize = 20);

public sealed record WorkOrderDto(
    Guid Id,
    string Number,
    string Title,
    string Description,
    WorkOrderSourceType SourceType,
    WorkOrderBusinessType BusinessType,
    WorkOrderPriority Priority,
    WorkOrderStatus Status,
    string ParkArea,
    Guid? RelatedAssetId,
    Guid? RelatedEventId,
    Guid? RelatedAlertId,
    string? AssigneeUserId,
    string? AssigneeName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyCollection<WorkOrderAttachmentDto> Attachments);

public sealed record WorkOrderAttachmentDto(Guid Id, string FileName, string Url, string ContentType);

public sealed record WorkOrderTimelineItemDto(
    Guid Id,
    string Action,
    WorkOrderStatus? FromStatus,
    WorkOrderStatus ToStatus,
    string OperatorUserId,
    string OperatorName,
    string? Comment,
    DateTimeOffset OccurredAt);

public interface IWorkOrderRepository
{
    Task AddAsync(Domain.WorkOrder entity, CancellationToken cancellationToken = default);

    Task<Domain.WorkOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<Domain.WorkOrder>> QueryAsync(WorkOrderQuery query, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IWorkOrderService
{
    Task<WorkOrderDto> CreateAsync(CreateWorkOrderRequest request, ActionContext context, CancellationToken cancellationToken = default);

    Task<PagedResult<WorkOrderDto>> QueryAsync(WorkOrderQuery query, CancellationToken cancellationToken = default);

    Task<WorkOrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<WorkOrderDto?> DispatchAsync(Guid id, DispatchWorkOrderRequest request, ActionContext context, CancellationToken cancellationToken = default);

    Task<WorkOrderDto?> AcceptAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default);

    Task<WorkOrderDto?> ArriveAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default);

    Task<WorkOrderDto?> StartAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default);

    Task<WorkOrderDto?> CompleteAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default);

    Task<WorkOrderDto?> VerifyAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default);

    Task<WorkOrderDto?> CloseAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<WorkOrderTimelineItemDto>> GetTimelineAsync(Guid id, CancellationToken cancellationToken = default);
}
