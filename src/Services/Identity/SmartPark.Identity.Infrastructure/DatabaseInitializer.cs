using Microsoft.EntityFrameworkCore;

namespace SmartPark.Identity.Infrastructure;

public sealed class IdentityDatabaseInitializer(IdentityDbContext dbContext)
{
    public Task InitializeAsync(CancellationToken cancellationToken = default)
        => dbContext.Database.MigrateAsync(cancellationToken);
}
