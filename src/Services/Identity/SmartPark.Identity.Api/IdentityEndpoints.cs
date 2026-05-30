using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SmartPark.Identity.Application;

namespace SmartPark.Identity.Api;

/// <summary>
/// 身份服务最小 API 端点定义。
/// 集中暴露登录、当前用户信息、用户列表和角色列表接口。
/// </summary>
public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/api/auth");
        auth.MapPost("/login", LoginAsync).AllowAnonymous();
        auth.MapGet("/me", GetCurrentUser).RequireAuthorization();

        var users = endpoints.MapGroup("/api").RequireAuthorization();
        users.MapGet("/users", GetUsersAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        users.MapGet("/roles", GetRolesAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(LoginRequest request, IIdentityService service, CancellationToken cancellationToken)
        => Results.Ok(await service.LoginAsync(request, cancellationToken));

    // 直接从 JWT Claims 回填当前登录人的基本信息，避免额外查库。
    private static IResult GetCurrentUser(ClaimsPrincipal user)
    {
        return Results.Ok(new
        {
            UserId = user.FindFirstValue(ClaimTypes.NameIdentifier),
            UserName = user.FindFirstValue(ClaimTypes.Name),
            DisplayName = user.FindFirstValue("display_name"),
            Roles = user.FindAll(ClaimTypes.Role).Select(item => item.Value).ToArray(),
            Permissions = user.FindAll("permission").Select(item => item.Value).ToArray()
        });
    }

    private static async Task<IResult> GetUsersAsync(IIdentityService service, CancellationToken cancellationToken)
        => Results.Ok(await service.GetUsersAsync(cancellationToken));

    private static async Task<IResult> GetRolesAsync(IIdentityService service, CancellationToken cancellationToken)
        => Results.Ok(await service.GetRolesAsync(cancellationToken));
}
