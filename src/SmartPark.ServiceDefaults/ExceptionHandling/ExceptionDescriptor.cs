using Microsoft.Extensions.Logging;

namespace SmartPark.ServiceDefaults.ExceptionHandling;

/// <summary>
/// 统一描述异常映射后的对外响应语义，供全局异常处理器生成标准 ProblemDetails。
/// </summary>
internal sealed record ExceptionDescriptor(
    int StatusCode,
    string Code,
    string Title,
    string Message,
    object? Details,
    LogLevel LogLevel);
