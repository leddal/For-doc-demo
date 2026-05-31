using Microsoft.EntityFrameworkCore;

namespace SmartPark.WorkOrder.Infrastructure;

public sealed class WorkOrderDatabaseInitializer(WorkOrderDbContext dbContext)
{
    public Task InitializeAsync(CancellationToken cancellationToken = default)
        => dbContext.Database.MigrateAsync(cancellationToken);
}
