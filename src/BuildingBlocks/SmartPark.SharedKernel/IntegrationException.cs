namespace SmartPark.SharedKernel;

/// <summary>
/// 集成异常，用于表达下游服务调用失败、响应异常或网络不可达等跨服务问题。
/// 这类异常通常会被统一映射为 502/503，避免把内部故障错误伪装成业务未找到。
/// </summary>
public sealed class IntegrationException : Exception
{
    public IntegrationException(string message, string code = "integration_error", int statusCode = 502, object? details = null)
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
