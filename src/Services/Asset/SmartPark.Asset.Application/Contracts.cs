using SmartPark.Asset.Domain;
using SmartPark.SharedContracts.Common;

namespace SmartPark.Asset.Application;

public sealed record AssetQuery(string? Keyword, AssetType? Type, AssetStatus? Status, int PageNumber = 1, int PageSize = 20);

public sealed record CreateAssetRequest(
    string AssetCode,
    string Name,
    AssetType Type,
    string Area,
    string Location,
    string? Model,
    string? Description);

public sealed record UpdateAssetRequest(
    string Name,
    AssetStatus Status,
    string Area,
    string Location,
    string? Model,
    string? Description,
    Guid? CurrentWorkOrderId);

public sealed record AssetDto(
    Guid Id,
    string AssetCode,
    string Name,
    AssetType Type,
    AssetStatus Status,
    string Area,
    string Location,
    string? Model,
    string? Description,
    Guid? CurrentWorkOrderId,
    DateTimeOffset CreatedAt);

public interface IAssetRepository
{
    Task AddAsync(ParkAsset entity, CancellationToken cancellationToken = default);

    Task<ParkAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<ParkAsset>> QueryAsync(AssetQuery query, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IAssetService
{
    Task<PagedResult<AssetDto>> QueryAsync(AssetQuery query, CancellationToken cancellationToken = default);

    Task<AssetDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AssetDto> CreateAsync(CreateAssetRequest request, CancellationToken cancellationToken = default);

    Task<AssetDto?> UpdateAsync(Guid id, UpdateAssetRequest request, CancellationToken cancellationToken = default);
}
