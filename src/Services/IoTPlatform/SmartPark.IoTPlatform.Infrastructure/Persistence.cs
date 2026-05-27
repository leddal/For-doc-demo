using Microsoft.EntityFrameworkCore;
using SmartPark.IoTPlatform.Application;
using SmartPark.IoTPlatform.Domain;
using SmartPark.SharedContracts.Common;
using SmartPark.SharedInfrastructure.Abstractions;
using SmartPark.SharedInfrastructure.Paging;
using SmartPark.SharedInfrastructure.Persistence;

namespace SmartPark.IoTPlatform.Infrastructure;

public sealed class IoTPlatformDbContext(
    DbContextOptions<IoTPlatformDbContext> options,
    IDateTimeProvider dateTimeProvider,
    ICurrentUser currentUser) : AppDbContextBase(options, dateTimeProvider, currentUser)
{
    public DbSet<MonitoringPoint> MonitoringPoints => Set<MonitoringPoint>();

    public DbSet<AlertRecord> Alerts => Set<AlertRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MonitoringPoint>(entity =>
        {
            entity.ToTable("monitoring_points");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(128).IsRequired();
            entity.Property(item => item.Unit).HasMaxLength(32).IsRequired();
            entity.Property(item => item.Area).HasMaxLength(128).IsRequired();
            entity.Property(item => item.StatusText).HasMaxLength(64).IsRequired();
            entity.Property(item => item.MetricType).HasConversion<string>().HasMaxLength(32);
        });

        modelBuilder.Entity<AlertRecord>(entity =>
        {
            entity.ToTable("alerts");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Title).HasMaxLength(128).IsRequired();
            entity.Property(item => item.Message).HasMaxLength(1000).IsRequired();
            entity.Property(item => item.Level).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(32);
        });
    }
}

public sealed class IoTRepository(IoTPlatformDbContext dbContext) : IIoTRepository
{
    public Task AddMonitoringPointAsync(MonitoringPoint entity, CancellationToken cancellationToken = default)
        => dbContext.MonitoringPoints.AddAsync(entity, cancellationToken).AsTask();

    public async Task<PagedResult<MonitoringPoint>> QueryMonitoringPointsAsync(MonitoringPointQuery query, CancellationToken cancellationToken = default)
    {
        var source = dbContext.MonitoringPoints.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            source = source.Where(item => item.Name.Contains(query.Keyword) || item.Area.Contains(query.Keyword));
        }

        if (query.MetricType.HasValue)
        {
            source = source.Where(item => item.MetricType == query.MetricType.Value);
        }

        return await source
            .OrderByDescending(item => item.CreatedAt)
            .ToPagedResultAsync(new PagedRequest { PageNumber = query.PageNumber, PageSize = query.PageSize }, cancellationToken);
    }

    public Task AddAlertAsync(AlertRecord entity, CancellationToken cancellationToken = default)
        => dbContext.Alerts.AddAsync(entity, cancellationToken).AsTask();

    public async Task<PagedResult<AlertRecord>> QueryAlertsAsync(AlertQuery query, CancellationToken cancellationToken = default)
    {
        var source = dbContext.Alerts.AsQueryable();

        if (query.Level.HasValue)
        {
            source = source.Where(item => item.Level == query.Level.Value);
        }

        if (query.Status.HasValue)
        {
            source = source.Where(item => item.Status == query.Status.Value);
        }

        return await source
            .OrderByDescending(item => item.CreatedAt)
            .ToPagedResultAsync(new PagedRequest { PageNumber = query.PageNumber, PageSize = query.PageSize }, cancellationToken);
    }

    public Task<int> CountMonitoringPointsAsync(CancellationToken cancellationToken = default)
        => dbContext.MonitoringPoints.CountAsync(cancellationToken);

    public Task<int> CountAlertsAsync(AlertLevel? level, AlertStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Alerts.AsQueryable();

        if (level.HasValue)
        {
            query = query.Where(item => item.Level == level.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(item => item.Status == status.Value);
        }

        return query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AlertRecord>> GetRecentAlertsAsync(int take, CancellationToken cancellationToken = default)
        => await dbContext.Alerts.OrderByDescending(item => item.CreatedAt).Take(take).ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
