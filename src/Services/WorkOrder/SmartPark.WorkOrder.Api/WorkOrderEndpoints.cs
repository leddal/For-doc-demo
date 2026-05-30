using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SmartPark.WorkOrder.Application;

namespace SmartPark.WorkOrder.Api;

/// <summary>
/// 工单服务最小 API 端点定义。
/// 重点暴露查询、详情、时间线以及完整状态流转接口。
/// 具体异常统一交给全局异常处理中间件生成标准错误响应。
/// </summary>
public static class WorkOrderEndpoints
{
    public static IEndpointRouteBuilder MapWorkOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/work-orders").RequireAuthorization();

        group.MapGet(string.Empty, QueryAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapGet("/{id:guid}/timeline", GetTimelineAsync);
        group.MapPost(string.Empty, CreateAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Dispatcher" });
        group.MapPost("/{id:guid}/dispatch", DispatchAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Dispatcher" });
        group.MapPost("/{id:guid}/accept", AcceptAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Operator" });
        group.MapPost("/{id:guid}/arrive", ArriveAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Operator" });
        group.MapPost("/{id:guid}/start", StartAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Operator" });
        group.MapPost("/{id:guid}/complete", CompleteAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Operator" });
        group.MapPost("/{id:guid}/verify", VerifyAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Reviewer" });
        group.MapPost("/{id:guid}/close", CloseAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Reviewer" });

        return endpoints;
    }

    // 查询接口支持按关键字、状态、业务类型、优先级和处理人筛选。
    private static async Task<IResult> QueryAsync(
        string? keyword,
        WorkOrder.Domain.WorkOrderStatus? status,
        WorkOrder.Domain.WorkOrderBusinessType? businessType,
        WorkOrder.Domain.WorkOrderPriority? priority,
        string? assigneeUserId,
        int pageNumber,
        int pageSize,
        IWorkOrderService service,
        CancellationToken cancellationToken)
        => Results.Ok(await service.QueryAsync(new WorkOrderQuery(keyword, status, businessType, priority, assigneeUserId, pageNumber == 0 ? 1 : pageNumber, pageSize == 0 ? 20 : pageSize), cancellationToken));

    private static async Task<IResult> GetByIdAsync(Guid id, IWorkOrderService service, CancellationToken cancellationToken)
        => Results.Ok(await service.GetByIdAsync(id, cancellationToken));

    private static async Task<IResult> GetTimelineAsync(Guid id, IWorkOrderService service, CancellationToken cancellationToken)
        => Results.Ok(await service.GetTimelineAsync(id, cancellationToken));

    private static async Task<IResult> CreateAsync(CreateWorkOrderRequest request, ClaimsPrincipal user, IWorkOrderService service, CancellationToken cancellationToken)
        => Results.Ok(await service.CreateAsync(request, ToContext(user), cancellationToken));

    private static Task<IResult> DispatchAsync(Guid id, DispatchWorkOrderRequest request, ClaimsPrincipal user, IWorkOrderService service, CancellationToken cancellationToken)
        => ExecuteAsync(() => service.DispatchAsync(id, request, ToContext(user), cancellationToken));

    private static Task<IResult> AcceptAsync(Guid id, ActionRequest request, ClaimsPrincipal user, IWorkOrderService service, CancellationToken cancellationToken)
        => ExecuteAsync(() => service.AcceptAsync(id, request, ToContext(user), cancellationToken));

    private static Task<IResult> ArriveAsync(Guid id, ActionRequest request, ClaimsPrincipal user, IWorkOrderService service, CancellationToken cancellationToken)
        => ExecuteAsync(() => service.ArriveAsync(id, request, ToContext(user), cancellationToken));

    private static Task<IResult> StartAsync(Guid id, ActionRequest request, ClaimsPrincipal user, IWorkOrderService service, CancellationToken cancellationToken)
        => ExecuteAsync(() => service.StartAsync(id, request, ToContext(user), cancellationToken));

    private static Task<IResult> CompleteAsync(Guid id, ActionRequest request, ClaimsPrincipal user, IWorkOrderService service, CancellationToken cancellationToken)
        => ExecuteAsync(() => service.CompleteAsync(id, request, ToContext(user), cancellationToken));

    private static Task<IResult> VerifyAsync(Guid id, ActionRequest request, ClaimsPrincipal user, IWorkOrderService service, CancellationToken cancellationToken)
        => ExecuteAsync(() => service.VerifyAsync(id, request, ToContext(user), cancellationToken));

    private static Task<IResult> CloseAsync(Guid id, ActionRequest request, ClaimsPrincipal user, IWorkOrderService service, CancellationToken cancellationToken)
        => ExecuteAsync(() => service.CloseAsync(id, request, ToContext(user), cancellationToken));

    // 把 ClaimsPrincipal 收敛为应用层需要的最小操作上下文，避免应用层直接依赖 ASP.NET。
    private static ActionContext ToContext(ClaimsPrincipal user)
        => new(
            user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system",
            user.FindFirstValue("display_name") ?? user.FindFirstValue(ClaimTypes.Name) ?? "system");

    private static async Task<IResult> ExecuteAsync(Func<Task<WorkOrderDto>> action)
        => Results.Ok(await action());
}
