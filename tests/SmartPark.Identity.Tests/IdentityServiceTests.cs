using SmartPark.Identity.Application;
using SmartPark.Identity.Domain;
using SmartPark.SharedContracts.Auth;
using SmartPark.SharedKernel;

namespace SmartPark.Identity.Tests;

public sealed class IdentityServiceTests
{
    [Fact]
    public async Task Login_Should_Return_Token_When_Password_Is_Valid()
    {
        var service = CreateService(CreateUser(), passwordResult: true);

        var token = await service.LoginAsync(new LoginRequest("admin", "SmartPark@123"));

        Assert.Equal("token", token.AccessToken);
    }

    [Fact]
    public async Task Login_Should_Throw_ValidationException_When_Request_Is_Invalid()
    {
        var service = CreateService(CreateUser(), passwordResult: true);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.LoginAsync(new LoginRequest(" ", string.Empty)));

        Assert.Equal("validation_failed", exception.Code);
        Assert.NotNull(exception.Details);
    }

    [Fact]
    public async Task Login_Should_Throw_AuthenticationFailedException_When_Password_Is_Invalid()
    {
        var service = CreateService(CreateUser(), passwordResult: false);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => service.LoginAsync(new LoginRequest("admin", "wrong")));
    }

    private static IdentityService CreateService(User user, bool passwordResult)
        => new(new FakeRepository(user), new FakePasswordService(passwordResult), new FakeTokenIssuer(), [new LoginRequestValidator()]);

    private static User CreateUser()
    {
        var role = new Role { Code = RoleCodes.Admin, Name = "管理员" };
        return new User
        {
            UserName = "admin",
            DisplayName = "管理员",
            PasswordHash = "hash",
            IsActive = true,
            UserRoles = [new UserRole { Role = role }]
        };
    }

    private sealed class FakeRepository(User user) : IIdentityRepository
    {
        public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
            => Task.FromResult<User?>(string.Equals(user.UserName, userName, StringComparison.OrdinalIgnoreCase) ? user : null);

        public Task<IReadOnlyCollection<User>> GetUsersAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<User>>([user]);

        public Task<IReadOnlyCollection<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Role>>(user.UserRoles.Select(item => item.Role).ToArray());
    }

    private sealed class FakePasswordService(bool result) : IPasswordService
    {
        public string HashPassword(User user, string password) => password;

        public bool VerifyPassword(User user, string passwordHash, string password) => result;
    }

    private sealed class FakeTokenIssuer : ITokenIssuer
    {
        public TokenResponse CreateToken(User user, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions)
            => new("token", DateTimeOffset.UtcNow.AddHours(1), user.Id.ToString(), user.UserName, user.DisplayName, roles, permissions);
    }
}
