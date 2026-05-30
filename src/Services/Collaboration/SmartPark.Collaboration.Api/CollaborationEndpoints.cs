using Microsoft.AspNetCore.Authorization;
using SmartPark.Collaboration.Application;
using SmartPark.Collaboration.Domain;

namespace SmartPark.Collaboration.Api;

/// <summary>
/// 协同服务端点。
/// 负责事件台账、公告发布以及事件转工单闭环。
/// </summary>
public static class CollaborationEndpoints
{
    public static IEndpointRouteBuilder MapCollaborationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var events = endpoints.MapGroup("/api/events").RequireAuthorization();
        events.MapGet(string.Empty, QueryEventsAsync);
        events.MapGet("/{id:guid}", GetEventAsync);
        events.MapPost(string.Empty, CreateEventAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Dispatcher" });
        events.MapPost("/{id:guid}/close", CloseEventAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Dispatcher,Reviewer" });
        events.MapPost("/{id:guid}/work-order", CreateWorkOrderAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Dispatcher" });

        var announcements = endpoints.MapGroup("/api/announcements").RequireAuthorization();
        announcements.MapGet(string.Empty, GetAnnouncementsAsync);
        announcements.MapPost(string.Empty, CreateAnnouncementAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Dispatcher" });

        return endpoints;
    }

    private static async Task<IResult> QueryEventsAsync(string? keyword, EventStatus? status, int pageNumber, int pageSize, ICollaborationService service, CancellationToken cancellationToken)
        => Results.Ok(await service.QueryEventsAsync(new EventQuery(keyword, status, pageNumber == 0 ? 1 : pageNumber, pageSize == 0 ? 20 : pageSize), cancellationToken));

    private static async Task<IResult> GetEventAsync(Guid id, ICollaborationService service, CancellationToken cancellationToken)
        => Results.Ok(await service.GetEventAsync(id, cancellationToken));

    private static async Task<IResult> CreateEventAsync(CreateEventRequest request, ICollaborationService service, CancellationToken cancellationToken)
        => Results.Ok(await service.CreateEventAsync(request, cancellationToken));

    private static async Task<IResult> CloseEventAsync(Guid id, CloseEventRequest request, ICollaborationService service, CancellationToken cancellationToken)
        => Results.Ok(await service.CloseEventAsync(id, request, cancellationToken));

    // 该接口会调用工单服务创建工单，并把回写结果绑定到当前事件台账。
    private static async Task<IResult> CreateWorkOrderAsync(Guid id, CreateWorkOrderFromEventRequest request, ICollaborationService service, CancellationToken cancellationToken)
        => Results.Ok(await service.CreateWorkOrderAsync(id, request, cancellationToken));

    private static async Task<IResult> GetAnnouncementsAsync(ICollaborationService service, CancellationToken cancellationToken)
        => Results.Ok(await service.GetAnnouncementsAsync(cancellationToken));

    private static async Task<IResult> CreateAnnouncementAsync(CreateAnnouncementRequest request, ICollaborationService service, CancellationToken cancellationToken)
        => Results.Ok(await service.CreateAnnouncementAsync(request, cancellationToken));
}
