using Microsoft.EntityFrameworkCore;
using SmartPark.SharedInfrastructure.Abstractions;
using SmartPark.SharedKernel;

namespace SmartPark.SharedInfrastructure.Persistence;

public abstract class AppDbContextBase(
    DbContextOptions options,
    IDateTimeProvider dateTimeProvider,
    ICurrentUser currentUser) : DbContext(options)
{
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAudit();
        return base.SaveChanges();
    }

    private void ApplyAudit()
    {
        var now = dateTimeProvider.UtcNow;
        var actor = currentUser.DisplayName ?? currentUser.UserName ?? "system";

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = actor;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = actor;
            }
        }
    }
}
