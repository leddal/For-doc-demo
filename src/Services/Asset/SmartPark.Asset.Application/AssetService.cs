using SmartPark.Asset.Domain;
using SmartPark.SharedContracts.Common;

namespace SmartPark.Asset.Application;

public sealed class AssetService(IAssetRepository repository) : IAssetService
{
    public async Task<PagedResult<AssetDto>> QueryAsync(AssetQuery query, CancellationToken cancellationToken = default)
    {
        var result = await repository.QueryAsync(query, cancellationToken);
        return new PagedResult<AssetDto>(result.Items.Select(Map).ToArray(), result.TotalCount, result.PageNumber, result.PageSize);
    }

    public async Task<AssetDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<AssetDto> CreateAsync(CreateAssetRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new ParkAsset
        {
            AssetCode = request.AssetCode,
            Name = request.Name,
            Type = request.Type,
            Area = request.Area,
            Location = request.Location,
            Model = request.Model,
            Description = request.Description
        };

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<AssetDto?> UpdateAsync(Guid id, UpdateAssetRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Name = request.Name;
        entity.Status = request.Status;
        entity.Area = request.Area;
        entity.Location = request.Location;
        entity.Model = request.Model;
        entity.Description = request.Description;
        entity.CurrentWorkOrderId = request.CurrentWorkOrderId;

        await repository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    private static AssetDto Map(ParkAsset entity)
        => new(entity.Id, entity.AssetCode, entity.Name, entity.Type, entity.Status, entity.Area, entity.Location, entity.Model, entity.Description, entity.CurrentWorkOrderId, entity.CreatedAt);
}
