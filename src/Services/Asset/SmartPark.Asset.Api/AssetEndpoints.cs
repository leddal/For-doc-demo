using Microsoft.AspNetCore.Authorization;
using SmartPark.Asset.Application;
using SmartPark.Asset.Domain;

namespace SmartPark.Asset.Api;

/// <summary>
/// 资产服务端点。
/// 提供资产分页查询、详情、创建和更新能力，供工单与协同模块关联资产台账。
/// </summary>
public static class AssetEndpoints
{
    public static IEndpointRouteBuilder MapAssetEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/assets").RequireAuthorization();
        group.MapGet(string.Empty, QueryAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost(string.Empty, CreateAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Operator" });
        group.MapPut("/{id:guid}", UpdateAsync).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Operator" });
        return endpoints;
    }

    private static async Task<IResult> QueryAsync(string? keyword, AssetType? type, AssetStatus? status, int pageNumber, int pageSize, IAssetService service, CancellationToken cancellationToken)
        => Results.Ok(await service.QueryAsync(new AssetQuery(keyword, type, status, pageNumber == 0 ? 1 : pageNumber, pageSize == 0 ? 20 : pageSize), cancellationToken));

    private static async Task<IResult> GetByIdAsync(Guid id, IAssetService service, CancellationToken cancellationToken)
    {
        var asset = await service.GetByIdAsync(id, cancellationToken);
        return asset is null ? Results.NotFound() : Results.Ok(asset);
    }

    private static async Task<IResult> CreateAsync(CreateAssetRequest request, IAssetService service, CancellationToken cancellationToken)
        => Results.Ok(await service.CreateAsync(request, cancellationToken));

    private static async Task<IResult> UpdateAsync(Guid id, UpdateAssetRequest request, IAssetService service, CancellationToken cancellationToken)
    {
        var asset = await service.UpdateAsync(id, request, cancellationToken);
        return asset is null ? Results.NotFound() : Results.Ok(asset);
    }
}
