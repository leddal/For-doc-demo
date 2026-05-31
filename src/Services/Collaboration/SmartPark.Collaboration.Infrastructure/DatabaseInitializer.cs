using Microsoft.EntityFrameworkCore;

namespace SmartPark.Collaboration.Infrastructure;

public sealed class CollaborationDatabaseInitializer(CollaborationDbContext dbContext)
{
    public Task InitializeAsync(CancellationToken cancellationToken = default)
        => dbContext.Database.MigrateAsync(cancellationToken);
}
