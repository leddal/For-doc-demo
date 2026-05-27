using SmartPark.SharedContracts.Common;
using SmartPark.WorkOrder.Domain;

namespace SmartPark.WorkOrder.Application;

public sealed class WorkOrderService(IWorkOrderRepository repository) : IWorkOrderService
{
    public async Task<WorkOrderDto> CreateAsync(CreateWorkOrderRequest request, ActionContext context, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var workOrder = Domain.WorkOrder.Create(
            $"WO-{now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
            request.Title,
            request.Description,
            request.SourceType,
            request.BusinessType,
            request.Priority,
            request.ParkArea,
            request.RelatedAssetId,
            request.RelatedEventId,
            request.RelatedAlertId,
            request.ReporterName,
            context.ActorId,
            context.ActorName,
            now);

        foreach (var attachment in request.Attachments ?? Array.Empty<WorkOrderAttachmentInput>())
        {
            workOrder.AddAttachment(attachment.FileName, attachment.Url, attachment.ContentType);
        }

        await repository.AddAsync(workOrder, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(workOrder);
    }

    public async Task<PagedResult<WorkOrderDto>> QueryAsync(WorkOrderQuery query, CancellationToken cancellationToken = default)
    {
        var result = await repository.QueryAsync(query, cancellationToken);
        return new PagedResult<WorkOrderDto>(result.Items.Select(Map).ToArray(), result.TotalCount, result.PageNumber, result.PageSize);
    }

    public async Task<WorkOrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workOrder = await repository.GetByIdAsync(id, cancellationToken);
        return workOrder is null ? null : Map(workOrder);
    }

    public Task<WorkOrderDto?> DispatchAsync(Guid id, DispatchWorkOrderRequest request, ActionContext context, CancellationToken cancellationToken = default)
        => ApplyAsync(id, workOrder => workOrder.Dispatch(context.ActorId, context.ActorName, request.AssigneeUserId, request.AssigneeName, request.Comment, DateTimeOffset.UtcNow), cancellationToken);

    public Task<WorkOrderDto?> AcceptAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default)
        => ApplyAsync(id, workOrder => workOrder.Accept(context.ActorId, context.ActorName, request.Comment, DateTimeOffset.UtcNow), cancellationToken);

    public Task<WorkOrderDto?> ArriveAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default)
        => ApplyAsync(id, workOrder => workOrder.Arrive(context.ActorId, context.ActorName, request.Comment, DateTimeOffset.UtcNow), cancellationToken);

    public Task<WorkOrderDto?> StartAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default)
        => ApplyAsync(id, workOrder => workOrder.Start(context.ActorId, context.ActorName, request.Comment, DateTimeOffset.UtcNow), cancellationToken);

    public Task<WorkOrderDto?> CompleteAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default)
        => ApplyAsync(id, workOrder => workOrder.Complete(context.ActorId, context.ActorName, request.Comment, DateTimeOffset.UtcNow), cancellationToken);

    public Task<WorkOrderDto?> VerifyAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default)
        => ApplyAsync(id, workOrder => workOrder.Verify(context.ActorId, context.ActorName, request.Comment, DateTimeOffset.UtcNow), cancellationToken);

    public Task<WorkOrderDto?> CloseAsync(Guid id, ActionRequest request, ActionContext context, CancellationToken cancellationToken = default)
        => ApplyAsync(id, workOrder => workOrder.Close(context.ActorId, context.ActorName, request.Comment, DateTimeOffset.UtcNow), cancellationToken);

    public async Task<IReadOnlyCollection<WorkOrderTimelineItemDto>> GetTimelineAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workOrder = await repository.GetByIdAsync(id, cancellationToken);
        return workOrder is null
            ? Array.Empty<WorkOrderTimelineItemDto>()
            : workOrder.ActionLogs
                .OrderBy(item => item.OccurredAt)
                .Select(item => new WorkOrderTimelineItemDto(item.Id, item.Action, item.FromStatus, item.ToStatus, item.OperatorUserId, item.OperatorName, item.Comment, item.OccurredAt))
                .ToArray();
    }

    private async Task<WorkOrderDto?> ApplyAsync(Guid id, Action<Domain.WorkOrder> apply, CancellationToken cancellationToken)
    {
        var workOrder = await repository.GetByIdAsync(id, cancellationToken);
        if (workOrder is null)
        {
            return null;
        }

        apply(workOrder);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(workOrder);
    }

    private static WorkOrderDto Map(Domain.WorkOrder workOrder)
    {
        return new WorkOrderDto(
            workOrder.Id,
            workOrder.Number,
            workOrder.Title,
            workOrder.Description,
            workOrder.SourceType,
            workOrder.BusinessType,
            workOrder.Priority,
            workOrder.Status,
            workOrder.ParkArea,
            workOrder.RelatedAssetId,
            workOrder.RelatedEventId,
            workOrder.RelatedAlertId,
            workOrder.AssigneeUserId,
            workOrder.AssigneeName,
            workOrder.CreatedAt,
            workOrder.CompletedAt,
            workOrder.Attachments.Select(item => new WorkOrderAttachmentDto(item.Id, item.FileName, item.Url, item.ContentType)).ToArray());
    }
}
