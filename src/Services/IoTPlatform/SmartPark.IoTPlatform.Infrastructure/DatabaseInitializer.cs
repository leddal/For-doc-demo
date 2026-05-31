using Microsoft.EntityFrameworkCore;

namespace SmartPark.IoTPlatform.Infrastructure;

public sealed class IoTDatabaseInitializer(IoTPlatformDbContext dbContext)
{
    public Task InitializeAsync(CancellationToken cancellationToken = default)
        => dbContext.Database.MigrateAsync(cancellationToken);
}
