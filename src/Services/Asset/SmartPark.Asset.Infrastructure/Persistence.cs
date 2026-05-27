using Microsoft.EntityFrameworkCore;
using SmartPark.Asset.Application;
using SmartPark.Asset.Domain;
using SmartPark.SharedContracts.Common;
using SmartPark.SharedInfrastructure.Abstractions;
using SmartPark.SharedInfrastructure.Paging;
using SmartPark.SharedInfrastructure.Persistence;

namespace SmartPark.Asset.Infrastructure;

public sealed class AssetDbContext(
    DbContextOptions<AssetDbContext> options,
    IDateTimeProvider dateTimeProvider,
    ICurrentUser currentUser) : AppDbContextBase(options, dateTimeProvider, currentUser)
{
    public DbSet<ParkAsset> Assets => Set<ParkAsset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ParkAsset>(entity =>
        {
            entity.ToTable("assets");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.AssetCode).HasMaxLength(32).IsRequired();
            entity.Property(item => item.Name).HasMaxLength(128).IsRequired();
            entity.Property(item => item.Area).HasMaxLength(128).IsRequired();
            entity.Property(item => item.Location).HasMaxLength(256).IsRequired();
            entity.Property(item => item.Model).HasMaxLength(128);
            entity.Property(item => item.Description).HasMaxLength(1000);
            entity.Property(item => item.Type).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(item => item.AssetCode).IsUnique();
        });
    }
}

public sealed class AssetRepository(AssetDbContext dbContext) : IAssetRepository
{
    public Task AddAsync(ParkAsset entity, CancellationToken cancellationToken = default)
        => dbContext.Assets.AddAsync(entity, cancellationToken).AsTask();

    public Task<ParkAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Assets.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    public async Task<PagedResult<ParkAsset>> QueryAsync(AssetQuery query, CancellationToken cancellationToken = default)
    {
        var source = dbContext.Assets.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            source = source.Where(item => item.AssetCode.Contains(query.Keyword) || item.Name.Contains(query.Keyword));
        }

        if (query.Type.HasValue)
        {
            source = source.Where(item => item.Type == query.Type.Value);
        }

        if (query.Status.HasValue)
        {
            source = source.Where(item => item.Status == query.Status.Value);
        }

        return await source
            .OrderByDescending(item => item.CreatedAt)
            .ToPagedResultAsync(new PagedRequest { PageNumber = query.PageNumber, PageSize = query.PageSize }, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
