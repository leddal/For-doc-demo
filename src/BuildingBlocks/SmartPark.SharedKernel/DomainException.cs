namespace SmartPark.SharedKernel;

/// <summary>
/// 领域异常，用于承载业务规则校验失败、流程冲突等可预期错误。
/// 统一附带错误码和状态码，便于在 API 层映射成标准错误响应。
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message, string code = "domain_error", int statusCode = 400, object? details = null)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
        Details = details;
    }

    public string Code { get; }

    public int StatusCode { get; }

    public object? Details { get; }
}
