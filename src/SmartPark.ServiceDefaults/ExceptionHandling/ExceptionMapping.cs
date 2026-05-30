using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartPark.SharedKernel;

namespace SmartPark.ServiceDefaults.ExceptionHandling;

/// <summary>
/// 负责把不同来源的异常统一映射为标准化的 HTTP 错误语义。
/// 这样各微服务只需要抛出异常，不必在端点中重复写 try/catch。
/// </summary>
internal static class ExceptionMapping
{
    public static ExceptionDescriptor Map(Exception exception)
        => exception switch
        {
            DomainException domainException => Create(
                domainException.StatusCode,
                domainException.Code,
                domainException.Message,
                domainException.Details,
                domainException.StatusCode >= StatusCodes.Status500InternalServerError ? LogLevel.Error : LogLevel.Warning),

            IntegrationException integrationException => Create(
                integrationException.StatusCode,
                integrationException.Code,
                integrationException.Message,
                integrationException.Details,
                LogLevel.Error),

            BadHttpRequestException badHttpRequestException => new(
                badHttpRequestException.StatusCode,
                "bad_http_request",
                "请求格式错误",
                string.IsNullOrWhiteSpace(badHttpRequestException.Message) ? "请求格式不正确。" : badHttpRequestException.Message,
                null,
                LogLevel.Warning),

            JsonException => new(
                StatusCodes.Status400BadRequest,
                "invalid_json_payload",
                "请求体解析失败",
                "请求体 JSON 格式不正确或字段类型不匹配。",
                null,
                LogLevel.Warning),

            UnauthorizedAccessException => MapStatusCode(StatusCodes.Status403Forbidden),

            HttpRequestException => Create(
                StatusCodes.Status503ServiceUnavailable,
                "upstream_service_unavailable",
                "依赖服务暂时不可用，请稍后重试。",
                null,
                LogLevel.Error),

            DbUpdateException => Create(
                StatusCodes.Status500InternalServerError,
                "database_update_failed",
                "数据库操作失败，请稍后重试。",
                null,
                LogLevel.Error),

            _ => Create(
                StatusCodes.Status500InternalServerError,
                "internal_server_error",
                "服务器发生未处理异常，请稍后重试。",
                null,
                LogLevel.Error)
        };

    public static ExceptionDescriptor MapStatusCode(int statusCode)
        => statusCode switch
        {
            StatusCodes.Status401Unauthorized => Create(
                StatusCodes.Status401Unauthorized,
                "unauthorized",
                "当前请求未通过身份验证，请先登录或提供有效令牌。",
                null,
                LogLevel.Warning),

            StatusCodes.Status403Forbidden => Create(
                StatusCodes.Status403Forbidden,
                "forbidden",
                "当前用户没有权限执行该操作。",
                null,
                LogLevel.Warning),

            StatusCodes.Status404NotFound => Create(
                StatusCodes.Status404NotFound,
                "resource_not_found",
                "请求的资源不存在。",
                null,
                LogLevel.Warning),

            StatusCodes.Status422UnprocessableEntity => Create(
                StatusCodes.Status422UnprocessableEntity,
                "validation_failed",
                "请求校验失败。",
                null,
                LogLevel.Warning),

            _ => Create(
                statusCode,
                "request_failed",
                "请求处理失败。",
                null,
                statusCode >= StatusCodes.Status500InternalServerError ? LogLevel.Error : LogLevel.Warning)
        };

    private static ExceptionDescriptor Create(int statusCode, string code, string message, object? details, LogLevel logLevel)
        => new(statusCode, code, GetTitle(statusCode), message, details, logLevel);

    private static string GetTitle(int statusCode)
        => statusCode switch
        {
            StatusCodes.Status400BadRequest => "请求参数错误",
            StatusCodes.Status401Unauthorized => "身份验证失败",
            StatusCodes.Status403Forbidden => "禁止访问",
            StatusCodes.Status404NotFound => "资源不存在",
            StatusCodes.Status409Conflict => "业务状态冲突",
            StatusCodes.Status422UnprocessableEntity => "业务规则校验失败",
            StatusCodes.Status502BadGateway => "下游服务处理失败",
            StatusCodes.Status503ServiceUnavailable => "依赖服务不可用",
            _ when statusCode >= StatusCodes.Status500InternalServerError => "服务器内部错误",
            _ => "请求处理失败"
        };
}
