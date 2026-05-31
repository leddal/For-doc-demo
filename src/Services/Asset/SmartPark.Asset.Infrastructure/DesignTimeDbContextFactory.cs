using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SmartPark.SharedInfrastructure.Abstractions;
using SmartPark.SharedInfrastructure.Runtime;

namespace SmartPark.Asset.Infrastructure;

public sealed class AssetDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AssetDbContext>
{
    public AssetDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AssetDbContext>();
        optionsBuilder.UseNpgsql(ResolveConnectionString(args));
        return new AssetDbContext(optionsBuilder.Options, new SystemDateTimeProvider(), DesignTimeCurrentUser.Instance);
    }

    private static string ResolveConnectionString(string[] args)
    {
        const string defaultConnectionString = "Host=localhost;Port=5432;Database=smartpark_asset;Username=postgres;Password=postgres";

        var commandLineConnection = args
            .FirstOrDefault(arg => arg.StartsWith("--connection=", StringComparison.OrdinalIgnoreCase))?
            .Split('=', 2)[1];

        if (!string.IsNullOrWhiteSpace(commandLineConnection))
        {
            return commandLineConnection;
        }

        return Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? defaultConnectionString;
    }

    private sealed class DesignTimeCurrentUser : ICurrentUser
    {
        public static DesignTimeCurrentUser Instance { get; } = new();

        public string? UserId => null;

        public string? UserName => "design-time";

        public string? DisplayName => "design-time";

        public IReadOnlyCollection<string> Roles => Array.Empty<string>();

        public bool IsAuthenticated => false;
    }
}
