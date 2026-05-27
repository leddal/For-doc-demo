using SmartPark.Identity.Domain;
using SmartPark.SharedContracts.Auth;

namespace SmartPark.Identity.Application;

public interface IIdentityRepository
{
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<User>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Role>> GetRolesAsync(CancellationToken cancellationToken = default);
}

public interface IPasswordService
{
    string HashPassword(User user, string password);

    bool VerifyPassword(User user, string passwordHash, string password);
}

public interface ITokenIssuer
{
    TokenResponse CreateToken(User user, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions);
}

public interface IIdentityService
{
    Task<TokenResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default);
}
