using SmartPark.Collaboration.Domain;
using SmartPark.SharedContracts.Common;
using SmartPark.SharedKernel;

namespace SmartPark.Collaboration.Application;

public sealed class CollaborationService(
    ICollaborationRepository repository,
    IWorkOrderGateway workOrderGateway,
    IEnumerable<IRequestValidator<CreateEventRequest>> createEventValidators,
    IEnumerable<IRequestValidator<CloseEventRequest>> closeEventValidators,
    IEnumerable<IRequestValidator<CreateWorkOrderFromEventRequest>> createWorkOrderValidators,
    IEnumerable<IRequestValidator<CreateAnnouncementRequest>> createAnnouncementValidators) : ICollaborationService
{
    public async Task<PagedResult<EventDto>> QueryEventsAsync(EventQuery query, CancellationToken cancellationToken = default)
    {
        var result = await repository.QueryEventsAsync(query, cancellationToken);
        return new PagedResult<EventDto>(result.Items.Select(MapEvent).ToArray(), result.TotalCount, result.PageNumber, result.PageSize);
    }

    public async Task<EventDto> GetEventAsync(Guid id, CancellationToken cancellationToken = default)
        => MapEvent(await GetRequiredEventAsync(id, cancellationToken));

    public async Task<EventDto> CreateEventAsync(CreateEventRequest request, CancellationToken cancellationToken = default)
    {
        createEventValidators.ValidateAndThrow(request);

        var now = DateTimeOffset.UtcNow;
        var entity = new EventRecord
        {
            Code = $"EVT-{now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
            Title = request.Title,
            Description = request.Description,
            Severity = request.Severity,
            Area = request.Area,
            RelatedAssetId = request.RelatedAssetId,
            RelatedAlertId = request.RelatedAlertId
        };

        await repository.AddEventAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return MapEvent(entity);
    }

    public async Task<EventDto> CloseEventAsync(Guid id, CloseEventRequest request, CancellationToken cancellationToken = default)
    {
        closeEventValidators.ValidateAndThrow(request);

        var entity = await GetRequiredEventAsync(id, cancellationToken);
        entity.Close(request.Note, DateTimeOffset.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return MapEvent(entity);
    }

    public async Task<CreatedWorkOrderInfo> CreateWorkOrderAsync(Guid eventId, CreateWorkOrderFromEventRequest request, CancellationToken cancellationToken = default)
    {
        createWorkOrderValidators.ValidateAndThrow(request);

        var entity = await GetRequiredEventAsync(eventId, cancellationToken);
        if (entity.WorkOrderId.HasValue && !string.IsNullOrWhiteSpace(entity.WorkOrderNumber))
        {
            return new CreatedWorkOrderInfo(entity.WorkOrderId.Value, entity.WorkOrderNumber);
        }

        var created = await workOrderGateway.CreateFromEventAsync(entity, request, cancellationToken);
        entity.BindWorkOrder(created.Id, created.Number);
        await repository.SaveChangesAsync(cancellationToken);
        return created;
    }

    public async Task<IReadOnlyCollection<AnnouncementDto>> GetAnnouncementsAsync(CancellationToken cancellationToken = default)
    {
        var announcements = await repository.GetAnnouncementsAsync(cancellationToken);
        return announcements.Select(MapAnnouncement).ToArray();
    }

    public async Task<AnnouncementDto> CreateAnnouncementAsync(CreateAnnouncementRequest request, CancellationToken cancellationToken = default)
    {
        createAnnouncementValidators.ValidateAndThrow(request);

        var entity = new Announcement
        {
            Title = request.Title,
            Content = request.Content,
            IsPublished = request.IsPublished,
            PublishedAt = DateTimeOffset.UtcNow
        };

        await repository.AddAnnouncementAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return MapAnnouncement(entity);
    }

    private async Task<EventRecord> GetRequiredEventAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await repository.GetEventAsync(id, cancellationToken);
        return entity ?? throw new NotFoundException($"未找到事件 {id}。");
    }

    private static EventDto MapEvent(EventRecord entity)
        => new(entity.Id, entity.Code, entity.Title, entity.Description, entity.Severity, entity.Status, entity.Area, entity.RelatedAssetId, entity.RelatedAlertId, entity.WorkOrderId, entity.WorkOrderNumber, entity.CreatedAt, entity.ClosedAt);

    private static AnnouncementDto MapAnnouncement(Announcement entity)
        => new(entity.Id, entity.Title, entity.Content, entity.IsPublished, entity.PublishedAt);
}
