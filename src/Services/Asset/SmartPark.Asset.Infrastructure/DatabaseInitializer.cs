using Microsoft.EntityFrameworkCore;
using SmartPark.Asset.Domain;

namespace SmartPark.Asset.Infrastructure;

public sealed class AssetDatabaseInitializer(AssetDbContext dbContext)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await dbContext.Assets.AnyAsync(cancellationToken))
        {
            return;
        }

        dbContext.Assets.AddRange(
            new ParkAsset
            {
                AssetCode = "DEV-001",
                Name = "东门摄像头",
                Type = AssetType.Device,
                Status = AssetStatus.Active,
                Area = "东门片区",
                Location = "东门入口立杆",
                Model = "Cam-Pro-X1",
                Description = "用于游客流量与安防监测。"
            },
            new ParkAsset
            {
                AssetCode = "INF-001",
                Name = "东门步道",
                Type = AssetType.Infrastructure,
                Status = AssetStatus.Active,
                Area = "东门片区",
                Location = "东门主步道",
                Description = "公园主要游览通道。"
            },
            new ParkAsset
            {
                AssetCode = "PLT-001",
                Name = "示范银杏树",
                Type = AssetType.Plant,
                Status = AssetStatus.Active,
                Area = "中心草坪",
                Location = "中心草坪东侧",
                Description = "重点养护植物。"
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
