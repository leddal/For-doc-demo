using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartPark.Collaboration.Application;
using SmartPark.Collaboration.Domain;
using SmartPark.SharedContracts.Auth;
using SmartPark.SharedKernel;

namespace SmartPark.Collaboration.Infrastructure;

public interface IInternalTokenProvider
{
    string CreateAdminToken();
}

public sealed class InternalTokenProvider(IOptions<JwtOptions> options) : IInternalTokenProvider
{
    private readonly JwtOptions _options = options.Value;

    public string CreateAdminToken()
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "collaboration-service"),
            new Claim(ClaimTypes.Name, "collaboration-service"),
            new Claim("display_name", "协同服务"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

internal sealed record WorkOrderCreateResponse(Guid Id, string Number);

internal sealed record DownstreamError(string? Message, string? Code);

public sealed class WorkOrderGateway(HttpClient httpClient, IInternalTokenProvider tokenProvider) : IWorkOrderGateway
{
    public async Task<CreatedWorkOrderInfo> CreateFromEventAsync(EventRecord entity, CreateWorkOrderFromEventRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/work-orders")
        {
            Content = JsonContent.Create(new
            {
                title = entity.Title,
                description = entity.Description,
                sourceType = 2,
                businessType = request.BusinessType,
                priority = request.Priority,
                parkArea = entity.Area,
                relatedAssetId = entity.RelatedAssetId,
                relatedEventId = entity.Id,
                relatedAlertId = entity.RelatedAlertId,
                reporterName = request.ReporterName ?? "事件联动",
                attachments = Array.Empty<object>()
            })
        };

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenProvider.CreateAdminToken());

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(message, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new IntegrationException(
                "工单服务响应超时，当前无法完成事件转工单。",
                code: "work_order_service_timeout",
                statusCode: 503,
                details: new { service = "work-order" });
        }
        catch (HttpRequestException)
        {
            throw new IntegrationException(
                "工单服务暂时不可用，当前无法完成事件转工单。",
                code: "work_order_service_unavailable",
                statusCode: 503,
                details: new { service = "work-order" });
        }

        using var _ = response;
        if (!response.IsSuccessStatusCode)
        {
            var error = await ReadErrorAsync(response, cancellationToken);
            var details = new
            {
                service = "work-order",
                downstreamStatus = (int)response.StatusCode,
                downstreamCode = error.Code
            };

            if (ShouldExposeAsDomainError(response.StatusCode))
            {
                throw new DomainException(
                    error.Message ?? "工单服务拒绝了当前创建请求。",
                    code: error.Code ?? "work_order_request_rejected",
                    statusCode: (int)response.StatusCode,
                    details: details);
            }

            throw new IntegrationException(
                error.Message ?? "工单服务处理创建请求失败。",
                code: error.Code ?? "work_order_service_error",
                statusCode: MapIntegrationStatusCode(response.StatusCode),
                details: details);
        }

        try
        {
            var workOrder = await response.Content.ReadFromJsonAsync<WorkOrderCreateResponse>(cancellationToken: cancellationToken);
            return workOrder is null
                ? throw new IntegrationException(
                    "工单服务返回了空响应，无法确认工单创建结果。",
                    code: "work_order_service_empty_response",
                    statusCode: 502,
                    details: new { service = "work-order", downstreamStatus = (int)response.StatusCode })
                : new CreatedWorkOrderInfo(workOrder.Id, workOrder.Number);
        }
        catch (JsonException)
        {
            throw new IntegrationException(
                "工单服务返回了无法识别的响应内容。",
                code: "work_order_service_invalid_response",
                statusCode: 502,
                details: new { service = "work-order", downstreamStatus = (int)response.StatusCode });
        }
    }

    private static bool ShouldExposeAsDomainError(HttpStatusCode statusCode)
        => statusCode is HttpStatusCode.BadRequest or HttpStatusCode.Conflict or HttpStatusCode.UnprocessableEntity;

    private static int MapIntegrationStatusCode(HttpStatusCode statusCode)
        => statusCode == HttpStatusCode.ServiceUnavailable
            ? 503
            : 502;

    private static async Task<DownstreamError> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentLength is 0)
        {
            return new DownstreamError(null, null);
        }

        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            if (stream == Stream.Null)
            {
                return new DownstreamError(null, null);
            }

            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = document.RootElement;
            return new DownstreamError(
                ReadString(root, "message") ?? ReadString(root, "detail") ?? ReadString(root, "title"),
                ReadString(root, "code"));
        }
        catch (JsonException)
        {
            return new DownstreamError(null, null);
        }
    }

    private static string? ReadString(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
