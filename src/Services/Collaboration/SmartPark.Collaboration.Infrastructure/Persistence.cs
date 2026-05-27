using Microsoft.EntityFrameworkCore;
using SmartPark.Collaboration.Application;
using SmartPark.Collaboration.Domain;
using SmartPark.SharedContracts.Common;
using SmartPark.SharedInfrastructure.Abstractions;
using SmartPark.SharedInfrastructure.Paging;
using SmartPark.SharedInfrastructure.Persistence;

namespace SmartPark.Collaboration.Infrastructure;

public sealed class CollaborationDbContext(
    DbContextOptions<CollaborationDbContext> options,
    IDateTimeProvider dateTimeProvider,
    ICurrentUser currentUser) : AppDbContextBase(options, dateTimeProvider, currentUser)
{
    public DbSet<EventRecord> Events => Set<EventRecord>();

    public DbSet<Announcement> Announcements => Set<Announcement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventRecord>(entity =>
        {
            entity.ToTable("events");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Code).HasMaxLength(32).IsRequired();
            entity.Property(item => item.Title).HasMaxLength(128).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(4000).IsRequired();
            entity.Property(item => item.Area).HasMaxLength(128).IsRequired();
            entity.Property(item => item.WorkOrderNumber).HasMaxLength(32);
            entity.Property(item => item.ClosedNote).HasMaxLength(1000);
            entity.Property(item => item.Severity).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(item => item.Code).IsUnique();
        });

        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.ToTable("announcements");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Title).HasMaxLength(128).IsRequired();
            entity.Property(item => item.Content).HasMaxLength(4000).IsRequired();
        });
    }
}

public sealed class CollaborationRepository(CollaborationDbContext dbContext) : ICollaborationRepository
{
    public Task AddEventAsync(EventRecord entity, CancellationToken cancellationToken = default)
        => dbContext.Events.AddAsync(entity, cancellationToken).AsTask();

    public Task<EventRecord?> GetEventAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Events.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    public async Task<PagedResult<EventRecord>> QueryEventsAsync(EventQuery query, CancellationToken cancellationToken = default)
    {
        var source = dbContext.Events.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            source = source.Where(item => item.Code.Contains(query.Keyword) || item.Title.Contains(query.Keyword));
        }

        if (query.Status.HasValue)
        {
            source = source.Where(item => item.Status == query.Status.Value);
        }

        return await source
            .OrderByDescending(item => item.CreatedAt)
            .ToPagedResultAsync(new PagedRequest { PageNumber = query.PageNumber, PageSize = query.PageSize }, cancellationToken);
    }

    public Task AddAnnouncementAsync(Announcement entity, CancellationToken cancellationToken = default)
        => dbContext.Announcements.AddAsync(entity, cancellationToken).AsTask();

    public async Task<IReadOnlyCollection<Announcement>> GetAnnouncementsAsync(CancellationToken cancellationToken = default)
        => await dbContext.Announcements
            .OrderByDescending(item => item.PublishedAt)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
