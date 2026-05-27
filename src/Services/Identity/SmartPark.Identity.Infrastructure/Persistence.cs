using Microsoft.EntityFrameworkCore;
using SmartPark.Identity.Application;
using SmartPark.Identity.Domain;
using SmartPark.SharedInfrastructure.Abstractions;
using SmartPark.SharedInfrastructure.Persistence;

namespace SmartPark.Identity.Infrastructure;

public sealed class IdentityDbContext(
    DbContextOptions<IdentityDbContext> options,
    IDateTimeProvider dateTimeProvider,
    ICurrentUser currentUser) : AppDbContextBase(options, dateTimeProvider, currentUser)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.UserName).HasMaxLength(64).IsRequired();
            entity.Property(item => item.DisplayName).HasMaxLength(64).IsRequired();
            entity.Property(item => item.Email).HasMaxLength(128);
            entity.Property(item => item.PasswordHash).HasMaxLength(512).IsRequired();
            entity.HasIndex(item => item.UserName).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Code).HasMaxLength(32).IsRequired();
            entity.Property(item => item.Name).HasMaxLength(64).IsRequired();
            entity.HasIndex(item => item.Code).IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(item => new { item.UserId, item.RoleId });
            entity.HasOne(item => item.User)
                .WithMany(user => user.UserRoles)
                .HasForeignKey(item => item.UserId);
            entity.HasOne(item => item.Role)
                .WithMany(role => role.UserRoles)
                .HasForeignKey(item => item.RoleId);
        });
    }
}

public sealed class IdentityRepository(IdentityDbContext dbContext) : IIdentityRepository
{
    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(item => item.UserRoles)
            .ThenInclude(item => item.Role)
            .FirstOrDefaultAsync(item => item.UserName == userName, cancellationToken);
    }

    public async Task<IReadOnlyCollection<User>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(item => item.UserRoles)
            .ThenInclude(item => item.Role)
            .OrderBy(item => item.UserName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Roles
            .OrderBy(item => item.Code)
            .ToListAsync(cancellationToken);
    }
}
