using Microsoft.EntityFrameworkCore;

namespace SmartPark.Asset.Infrastructure;

public sealed class AssetDatabaseInitializer(AssetDbContext dbContext)
{
    public Task InitializeAsync(CancellationToken cancellationToken = default)
        => dbContext.Database.MigrateAsync(cancellationToken);
}
