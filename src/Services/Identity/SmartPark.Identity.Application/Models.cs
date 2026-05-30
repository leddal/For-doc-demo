using SmartPark.SharedKernel;

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

public sealed class LoginRequestValidator : IRequestValidator<LoginRequest>
{
    public IReadOnlyCollection<ValidationError> Validate(LoginRequest request)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            errors.Add(new ValidationError("userName", "用户名不能为空。"));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add(new ValidationError("password", "密码不能为空。"));
        }

        return errors;
    }
}
