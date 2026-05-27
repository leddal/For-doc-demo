using Microsoft.EntityFrameworkCore;
using SmartPark.SharedContracts.Common;
using SmartPark.SharedInfrastructure.Abstractions;
using SmartPark.SharedInfrastructure.Paging;
using SmartPark.SharedInfrastructure.Persistence;
using SmartPark.WorkOrder.Application;
using SmartPark.WorkOrder.Domain;

namespace SmartPark.WorkOrder.Infrastructure;

public sealed class WorkOrderDbContext(
    DbContextOptions<WorkOrderDbContext> options,
    IDateTimeProvider dateTimeProvider,
    ICurrentUser currentUser) : AppDbContextBase(options, dateTimeProvider, currentUser)
{
    public DbSet<Domain.WorkOrder> WorkOrders => Set<Domain.WorkOrder>();

    public DbSet<WorkOrderActionLog> WorkOrderActionLogs => Set<WorkOrderActionLog>();

    public DbSet<WorkOrderAttachment> WorkOrderAttachments => Set<WorkOrderAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.WorkOrder>(entity =>
        {
            entity.ToTable("work_orders");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Number).HasMaxLength(32).IsRequired();
            entity.Property(item => item.Title).HasMaxLength(128).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(4000).IsRequired();
            entity.Property(item => item.ParkArea).HasMaxLength(128).IsRequired();
            entity.Property(item => item.ReporterName).HasMaxLength(64);
            entity.Property(item => item.AssigneeName).HasMaxLength(64);
            entity.Property(item => item.DispatcherName).HasMaxLength(64);
            entity.Property(item => item.ReviewerName).HasMaxLength(64);
            entity.Property(item => item.CompletionNote).HasMaxLength(1000);
            entity.Property(item => item.VerificationNote).HasMaxLength(1000);
            entity.Property(item => item.CloseNote).HasMaxLength(1000);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.SourceType).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.BusinessType).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Priority).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(item => item.Number).IsUnique();
            entity.HasMany(item => item.ActionLogs)
                .WithOne(item => item.WorkOrder)
                .HasForeignKey(item => item.WorkOrderId);
            entity.HasMany(item => item.Attachments)
                .WithOne(item => item.WorkOrder)
                .HasForeignKey(item => item.WorkOrderId);
        });

        modelBuilder.Entity<WorkOrderActionLog>(entity =>
        {
            entity.ToTable("work_order_action_logs");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Action).HasMaxLength(32).IsRequired();
            entity.Property(item => item.OperatorUserId).HasMaxLength(64).IsRequired();
            entity.Property(item => item.OperatorName).HasMaxLength(64).IsRequired();
            entity.Property(item => item.Comment).HasMaxLength(1000);
            entity.Property(item => item.FromStatus).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.ToStatus).HasConversion<string>().HasMaxLength(32);
        });

        modelBuilder.Entity<WorkOrderAttachment>(entity =>
        {
            entity.ToTable("work_order_attachments");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.FileName).HasMaxLength(256).IsRequired();
            entity.Property(item => item.Url).HasMaxLength(1024).IsRequired();
            entity.Property(item => item.ContentType).HasMaxLength(128).IsRequired();
        });
    }
}

public sealed class WorkOrderRepository(WorkOrderDbContext dbContext) : IWorkOrderRepository
{
    public Task AddAsync(Domain.WorkOrder entity, CancellationToken cancellationToken = default)
        => dbContext.WorkOrders.AddAsync(entity, cancellationToken).AsTask();

    public async Task<Domain.WorkOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.WorkOrders
            .Include(item => item.ActionLogs)
            .Include(item => item.Attachments)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Domain.WorkOrder>> QueryAsync(WorkOrderQuery query, CancellationToken cancellationToken = default)
    {
        var source = dbContext.WorkOrders
            .Include(item => item.Attachments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            source = source.Where(item => item.Number.Contains(query.Keyword) || item.Title.Contains(query.Keyword));
        }

        if (query.Status.HasValue)
        {
            source = source.Where(item => item.Status == query.Status.Value);
        }

        if (query.BusinessType.HasValue)
        {
            source = source.Where(item => item.BusinessType == query.BusinessType.Value);
        }

        if (query.Priority.HasValue)
        {
            source = source.Where(item => item.Priority == query.Priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.AssigneeUserId))
        {
            source = source.Where(item => item.AssigneeUserId == query.AssigneeUserId);
        }

        return await source
            .OrderByDescending(item => item.CreatedAt)
            .ToPagedResultAsync(new PagedRequest { PageNumber = query.PageNumber, PageSize = query.PageSize }, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
