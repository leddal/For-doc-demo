using Microsoft.EntityFrameworkCore;
using SmartPark.Asset.Domain;
using SmartPark.Asset.Infrastructure;
using SmartPark.DbSeeder.Options;

namespace SmartPark.DbSeeder.Seeders;

public sealed class AssetSeeder(AssetDbContext dbContext)
{
    public async Task<AssetSeedResult> SeedAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        var specs = new[]
        {
            new AssetSeedSpec(DemoSeedData.CameraAssetCode, "东门摄像头", AssetType.Device, AssetStatus.Active, "东门片区", "东门入口立杆", "Cam-Pro-X1", "用于游客流量与安防监测。"),
            new AssetSeedSpec(DemoSeedData.WalkwayAssetCode, "东门步道", AssetType.Infrastructure, AssetStatus.Active, "东门片区", "东门主步道", null, "公园主要游览通道。"),
            new AssetSeedSpec(DemoSeedData.GinkgoAssetCode, "示范银杏树", AssetType.Plant, AssetStatus.Active, "中心草坪", "中心草坪东侧", null, "重点养护植物。")
        };

        var assetCodes = specs.Select(item => item.AssetCode).ToArray();
        var assets = await dbContext.Assets
            .Where(item => assetCodes.Contains(item.AssetCode))
            .ToDictionaryAsync(item => item.AssetCode, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var spec in specs)
        {
            if (!assets.TryGetValue(spec.AssetCode, out var asset))
            {
                asset = new ParkAsset
                {
                    AssetCode = spec.AssetCode
                };

                dbContext.Assets.Add(asset);
                assets[spec.AssetCode] = asset;
            }

            asset.Name = spec.Name;
            asset.Type = spec.Type;
            asset.Status = spec.Status;
            asset.Area = spec.Area;
            asset.Location = spec.Location;
            asset.Model = spec.Model;
            asset.Description = spec.Description;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AssetSeedResult(
            assets[DemoSeedData.CameraAssetCode].Id,
            assets[DemoSeedData.WalkwayAssetCode].Id,
            assets[DemoSeedData.GinkgoAssetCode].Id);
    }

    public async Task BindWorkOrderAsync(Guid assetId, Guid workOrderId, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        var asset = await dbContext.Assets.FirstOrDefaultAsync(item => item.Id == assetId, cancellationToken);
        if (asset is null || asset.CurrentWorkOrderId == workOrderId || asset.CurrentWorkOrderId.HasValue)
        {
            return;
        }

        asset.CurrentWorkOrderId = workOrderId;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed record AssetSeedResult(Guid CameraAssetId, Guid WalkwayAssetId, Guid GinkgoAssetId);

file sealed record AssetSeedSpec(
    string AssetCode,
    string Name,
    AssetType Type,
    AssetStatus Status,
    string Area,
    string Location,
    string? Model,
    string Description);
