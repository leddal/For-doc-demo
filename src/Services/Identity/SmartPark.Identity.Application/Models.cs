namespace SmartPark.Identity.Application;

public sealed record LoginRequest(string UserName, string Password);

public sealed record UserDto(
    Guid Id,
    string UserName,
    string DisplayName,
    string Email,
    bool IsActive,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);

public sealed record RoleDto(
    Guid Id,
    string Code,
    string Name,
    IReadOnlyCollection<string> Permissions);
