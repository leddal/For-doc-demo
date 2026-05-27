using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SmartPark.SharedInfrastructure.Abstractions;

namespace SmartPark.SharedInfrastructure.Runtime;

public sealed class HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string? UserId => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);

    public string? DisplayName => httpContextAccessor.HttpContext?.User.FindFirstValue("display_name") ?? UserName;

    public IReadOnlyCollection<string> Roles =>
        httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray()
        ?? Array.Empty<string>();

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
