namespace SmartPark.SharedInfrastructure.Abstractions;

public interface ICurrentUser
{
    string? UserId { get; }

    string? UserName { get; }

    string? DisplayName { get; }

    IReadOnlyCollection<string> Roles { get; }

    bool IsAuthenticated { get; }
}
