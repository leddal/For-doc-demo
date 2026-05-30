namespace SmartPark.SharedKernel;

public sealed class AuthenticationFailedException : DomainException
{
    public AuthenticationFailedException(string message = "用户名或密码错误。", object? details = null)
        : base(message, "authentication_failed", 401, details)
    {
    }
}
