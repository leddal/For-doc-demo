using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartPark.Collaboration.Application;
using SmartPark.Collaboration.Domain;
using SmartPark.SharedContracts.Auth;

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

public sealed class WorkOrderGateway(HttpClient httpClient, IInternalTokenProvider tokenProvider) : IWorkOrderGateway
{
    public async Task<CreatedWorkOrderInfo?> CreateFromEventAsync(EventRecord entity, CreateWorkOrderFromEventRequest request, CancellationToken cancellationToken = default)
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
        var response = await httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var workOrder = await response.Content.ReadFromJsonAsync<WorkOrderCreateResponse>(cancellationToken: cancellationToken);
        return workOrder is null ? null : new CreatedWorkOrderInfo(workOrder.Id, workOrder.Number);
    }
}
