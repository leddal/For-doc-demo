using SmartPark.Collaboration.Domain;
using SmartPark.SharedContracts.Common;
using SmartPark.SharedKernel;

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
    // 网关成功时必须返回已创建工单；任何失败都应抛出明确异常，不能再退化为 null。
    Task<CreatedWorkOrderInfo> CreateFromEventAsync(EventRecord entity, CreateWorkOrderFromEventRequest request, CancellationToken cancellationToken = default);
}

public interface ICollaborationService
{
    Task<PagedResult<EventDto>> QueryEventsAsync(EventQuery query, CancellationToken cancellationToken = default);

    Task<EventDto> GetEventAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EventDto> CreateEventAsync(CreateEventRequest request, CancellationToken cancellationToken = default);

    Task<EventDto> CloseEventAsync(Guid id, CloseEventRequest request, CancellationToken cancellationToken = default);

    Task<CreatedWorkOrderInfo> CreateWorkOrderAsync(Guid eventId, CreateWorkOrderFromEventRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AnnouncementDto>> GetAnnouncementsAsync(CancellationToken cancellationToken = default);

    Task<AnnouncementDto> CreateAnnouncementAsync(CreateAnnouncementRequest request, CancellationToken cancellationToken = default);
}

public sealed class CreateEventRequestValidator : IRequestValidator<CreateEventRequest>
{
    public IReadOnlyCollection<ValidationError> Validate(CreateEventRequest request)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors.Add(new ValidationError("title", "事件标题不能为空。"));
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            errors.Add(new ValidationError("description", "事件描述不能为空。"));
        }

        if (!Enum.IsDefined(request.Severity))
        {
            errors.Add(new ValidationError("severity", "事件严重程度不合法。"));
        }

        if (string.IsNullOrWhiteSpace(request.Area))
        {
            errors.Add(new ValidationError("area", "事件所属区域不能为空。"));
        }

        return errors;
    }
}

public sealed class CloseEventRequestValidator : IRequestValidator<CloseEventRequest>
{
    public IReadOnlyCollection<ValidationError> Validate(CloseEventRequest request)
    {
        if (request.Note is not null && string.IsNullOrWhiteSpace(request.Note))
        {
            return [new ValidationError("note", "关闭说明如果提供则不能为空白字符串。")];
        }

        return [];
    }
}

public sealed class CreateWorkOrderFromEventRequestValidator : IRequestValidator<CreateWorkOrderFromEventRequest>
{
    public IReadOnlyCollection<ValidationError> Validate(CreateWorkOrderFromEventRequest request)
    {
        var errors = new List<ValidationError>();

        if (request.BusinessType is < 1 or > 5)
        {
            errors.Add(new ValidationError("businessType", "事件转工单的业务类型不合法。"));
        }

        if (request.Priority is < 1 or > 4)
        {
            errors.Add(new ValidationError("priority", "事件转工单的优先级不合法。"));
        }

        if (request.ReporterName is not null && string.IsNullOrWhiteSpace(request.ReporterName))
        {
            errors.Add(new ValidationError("reporterName", "上报人姓名如果提供则不能为空白字符串。"));
        }

        return errors;
    }
}

public sealed class CreateAnnouncementRequestValidator : IRequestValidator<CreateAnnouncementRequest>
{
    public IReadOnlyCollection<ValidationError> Validate(CreateAnnouncementRequest request)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors.Add(new ValidationError("title", "公告标题不能为空。"));
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            errors.Add(new ValidationError("content", "公告内容不能为空。"));
        }

        return errors;
    }
}
