using SmartPark.Identity.Domain;
using SmartPark.SharedContracts.Auth;

namespace SmartPark.Identity.Application;

public sealed class IdentityService(
    IIdentityRepository repository,
    IPasswordService passwordService,
    ITokenIssuer tokenIssuer) : IIdentityService
{
    public async Task<TokenResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetByUserNameAsync(request.UserName, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        if (!passwordService.VerifyPassword(user, user.PasswordHash, request.Password))
        {
            return null;
        }

        var roles = user.UserRoles.Select(item => item.Role.Code).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var permissions = roles
            .SelectMany(role => RolePermissionMatrix.Definitions.GetValueOrDefault(role, Array.Empty<string>()))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return tokenIssuer.CreateToken(user, roles, permissions);
    }

    public async Task<IReadOnlyCollection<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await repository.GetUsersAsync(cancellationToken);
        return users.Select(MapUser).ToArray();
    }

    public async Task<IReadOnlyCollection<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await repository.GetRolesAsync(cancellationToken);
        return roles
            .Select(role => new RoleDto(
                role.Id,
                role.Code,
                role.Name,
                RolePermissionMatrix.Definitions.GetValueOrDefault(role.Code, Array.Empty<string>())))
            .ToArray();
    }

    private static UserDto MapUser(User user)
    {
        var roles = user.UserRoles.Select(item => item.Role.Code).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var permissions = roles
            .SelectMany(role => RolePermissionMatrix.Definitions.GetValueOrDefault(role, Array.Empty<string>()))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new UserDto(user.Id, user.UserName, user.DisplayName, user.Email, user.IsActive, roles, permissions);
    }
}
