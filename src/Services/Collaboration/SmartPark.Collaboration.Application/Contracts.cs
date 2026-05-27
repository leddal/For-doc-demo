using SmartPark.Collaboration.Domain;
using SmartPark.SharedContracts.Common;

namespace SmartPark.Collaboration.Application;

public sealed record EventQuery(string? Keyword, EventStatus? Status, int PageNumber = 1, int PageSize = 20);

public sealed record CreateEventRequest(
    string Title,
    string Description,
    EventSeverity Severity,
    string Area,
    Guid? RelatedAssetId,
    Guid? RelatedAlertId);

public sealed record CloseEventRequest(string? Note);

public sealed record CreateWorkOrderFromEventRequest(int BusinessType = 1, int Priority = 3, string? ReporterName = null);

public sealed record CreateAnnouncementRequest(string Title, string Content, bool IsPublished = true);

public sealed record EventDto(
    Guid Id,
    string Code,
    string Title,
    string Description,
    EventSeverity Severity,
    EventStatus Status,
    string Area,
    Guid? RelatedAssetId,
    Guid? RelatedAlertId,
    Guid? WorkOrderId,
    string? WorkOrderNumber,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ClosedAt);

public sealed record AnnouncementDto(Guid Id, string Title, string Content, bool IsPublished, DateTimeOffset PublishedAt);

public sealed record CreatedWorkOrderInfo(Guid Id, string Number);

public interface ICollaborationRepository
{
    Task AddEventAsync(EventRecord entity, CancellationToken cancellationToken = default);

    Task<EventRecord?> GetEventAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<EventRecord>> QueryEventsAsync(EventQuery query, CancellationToken cancellationToken = default);

    Task AddAnnouncementAsync(Announcement entity, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Announcement>> GetAnnouncementsAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IWorkOrderGateway
{
    Task<CreatedWorkOrderInfo?> CreateFromEventAsync(EventRecord entity, CreateWorkOrderFromEventRequest request, CancellationToken cancellationToken = default);
}

public interface ICollaborationService
{
    Task<PagedResult<EventDto>> QueryEventsAsync(EventQuery query, CancellationToken cancellationToken = default);

    Task<EventDto?> GetEventAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EventDto> CreateEventAsync(CreateEventRequest request, CancellationToken cancellationToken = default);

    Task<EventDto?> CloseEventAsync(Guid id, CloseEventRequest request, CancellationToken cancellationToken = default);

    Task<CreatedWorkOrderInfo?> CreateWorkOrderAsync(Guid eventId, CreateWorkOrderFromEventRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AnnouncementDto>> GetAnnouncementsAsync(CancellationToken cancellationToken = default);

    Task<AnnouncementDto> CreateAnnouncementAsync(CreateAnnouncementRequest request, CancellationToken cancellationToken = default);
}
