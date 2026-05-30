using SmartPark.SharedContracts.Common;
using SmartPark.SharedKernel;
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

    Task<WorkOrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<WorkOrderDto> DispatchAsync(Guid id, DispatchWorkOrderRequest request, ActionContext context, CancellationToken cancellationToken = default);

    Task<WorkOrderDto> AcceptAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default);

    Task<WorkOrderDto> ArriveAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default);

    Task<WorkOrderDto> StartAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default);

    Task<WorkOrderDto> CompleteAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default);

    Task<WorkOrderDto> VerifyAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default);

    Task<WorkOrderDto> CloseAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<WorkOrderTimelineItemDto>> GetTimelineAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class CreateWorkOrderRequestValidator : IRequestValidator<CreateWorkOrderRequest>
{
    public IReadOnlyCollection<ValidationError> Validate(CreateWorkOrderRequest request)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors.Add(new ValidationError("title", "工单标题不能为空。"));
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            errors.Add(new ValidationError("description", "工单描述不能为空。"));
        }

        if (!Enum.IsDefined(request.SourceType))
        {
            errors.Add(new ValidationError("sourceType", "工单来源类型不合法。"));
        }

        if (!Enum.IsDefined(request.BusinessType))
        {
            errors.Add(new ValidationError("businessType", "工单业务类型不合法。"));
        }

        if (!Enum.IsDefined(request.Priority))
        {
            errors.Add(new ValidationError("priority", "工单优先级不合法。"));
        }

        if (string.IsNullOrWhiteSpace(request.ParkArea))
        {
            errors.Add(new ValidationError("parkArea", "所属园区区域不能为空。"));
        }

        var attachments = request.Attachments?.ToArray() ?? [];
        for (var index = 0; index < attachments.Length; index++)
        {
            var attachment = attachments[index];
            if (string.IsNullOrWhiteSpace(attachment.FileName))
            {
                errors.Add(new ValidationError($"attachments[{index}].fileName", "附件文件名不能为空。"));
            }

            if (string.IsNullOrWhiteSpace(attachment.Url))
            {
                errors.Add(new ValidationError($"attachments[{index}].url", "附件地址不能为空。"));
            }

            if (string.IsNullOrWhiteSpace(attachment.ContentType))
            {
                errors.Add(new ValidationError($"attachments[{index}].contentType", "附件内容类型不能为空。"));
            }
        }

        return errors;
    }
}

public sealed class DispatchWorkOrderRequestValidator : IRequestValidator<DispatchWorkOrderRequest>
{
    public IReadOnlyCollection<ValidationError> Validate(DispatchWorkOrderRequest request)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.AssigneeUserId))
        {
            errors.Add(new ValidationError("assigneeUserId", "处理人用户标识不能为空。"));
        }

        if (string.IsNullOrWhiteSpace(request.AssigneeName))
        {
            errors.Add(new ValidationError("assigneeName", "处理人姓名不能为空。"));
        }

        if (request.Comment is not null && string.IsNullOrWhiteSpace(request.Comment))
        {
            errors.Add(new ValidationError("comment", "派单说明如果提供则不能为空白字符串。"));
        }

        return errors;
    }
}

public sealed class ActionRequestValidator : IRequestValidator<ActionRequest>
{
    public IReadOnlyCollection<ValidationError> Validate(ActionRequest request)
    {
        if (request.Comment is not null && string.IsNullOrWhiteSpace(request.Comment))
        {
            return [new ValidationError("comment", "处理说明如果提供则不能为空白字符串。")];
        }

        return [];
    }
}
