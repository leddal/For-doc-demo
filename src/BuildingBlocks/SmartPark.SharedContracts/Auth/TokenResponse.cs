namespace SmartPark.SharedContracts.Auth;

public sealed record TokenResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string UserId,
    string UserName,
    string DisplayName,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
