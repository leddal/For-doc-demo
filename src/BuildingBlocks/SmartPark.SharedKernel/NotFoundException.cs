namespace SmartPark.SharedKernel;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message = "请求的资源不存在。", string code = "resource_not_found", object? details = null)
        : base(message, code, 404, details)
    {
    }
}
