using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartPark.Asset.Infrastructure;
using SmartPark.Collaboration.Domain;
using SmartPark.Collaboration.Infrastructure;
using SmartPark.DbSeeder.Options;
using SmartPark.DbSeeder.Seeders;
using SmartPark.Identity.Application;
using SmartPark.Identity.Infrastructure;
using SmartPark.IoTPlatform.Infrastructure;
using SmartPark.SharedInfrastructure.Abstractions;
using SmartPark.SharedInfrastructure.Runtime;
using SmartPark.WorkOrder.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);
var seederOptions = SeederOptions.FromConfiguration(builder.Configuration);
var selectedModules = ExpandModules(ParseModules(builder.Configuration["modules"] ?? builder.Configuration["Seeder:Modules"]));

builder.Services.AddSingleton(seederOptions);
builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddSingleton<ICurrentUser, SeederCurrentUser>();
builder.Services.AddScoped<IPasswordService, PasswordService>();

builder.Services.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(seederOptions.IdentityConnectionString));
builder.Services.AddDbContext<AssetDbContext>(options => options.UseNpgsql(seederOptions.AssetConnectionString));
builder.Services.AddDbContext<IoTPlatformDbContext>(options => options.UseNpgsql(seederOptions.IoTConnectionString));
builder.Services.AddDbContext<CollaborationDbContext>(options => options.UseNpgsql(seederOptions.CollaborationConnectionString));
builder.Services.AddDbContext<WorkOrderDbContext>(options => options.UseNpgsql(seederOptions.WorkOrderConnectionString));

builder.Services.AddScoped<IdentitySeeder>();
builder.Services.AddScoped<AssetSeeder>();
builder.Services.AddScoped<IoTSeeder>();
builder.Services.AddScoped<CollaborationSeeder>();
builder.Services.AddScoped<WorkOrderSeeder>();

Console.WriteLine($"Selected modules: {string.Join(", ", selectedModules.OrderBy(static module => module))}");

using var host = builder.Build();
await using var scope = host.Services.CreateAsyncScope();

var identitySeeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
var assetSeeder = scope.ServiceProvider.GetRequiredService<AssetSeeder>();
var ioTSeeder = scope.ServiceProvider.GetRequiredService<IoTSeeder>();
var collaborationSeeder = scope.ServiceProvider.GetRequiredService<CollaborationSeeder>();
var workOrderSeeder = scope.ServiceProvider.GetRequiredService<WorkOrderSeeder>();

AssetSeedResult? assetResult = null;
IoTSeedResult? ioTResult = null;
CollaborationSeedResult? collaborationResult = null;

if (selectedModules.Contains(SeederModule.Identity))
{
    Console.WriteLine("Seeding Identity...");
    await identitySeeder.SeedAsync();
}

if (selectedModules.Contains(SeederModule.Asset))
{
    Console.WriteLine("Seeding Asset...");
    assetResult = await assetSeeder.SeedAsync();
}

if (selectedModules.Contains(SeederModule.IoTPlatform))
{
    assetResult ??= await assetSeeder.SeedAsync();
    Console.WriteLine("Seeding IoTPlatform...");
    ioTResult = await ioTSeeder.SeedAsync(assetResult);
}

if (selectedModules.Contains(SeederModule.Collaboration))
{
    assetResult ??= await assetSeeder.SeedAsync();
    ioTResult ??= await ioTSeeder.SeedAsync(assetResult);
    Console.WriteLine("Seeding Collaboration...");
    collaborationResult = await collaborationSeeder.SeedAsync(assetResult, ioTResult);
}

if (selectedModules.Contains(SeederModule.WorkOrder))
{
    assetResult ??= await assetSeeder.SeedAsync();
    ioTResult ??= await ioTSeeder.SeedAsync(assetResult);
    collaborationResult ??= await collaborationSeeder.SeedAsync(assetResult, ioTResult);
    Console.WriteLine("Seeding WorkOrder...");
    var workOrderResult = await workOrderSeeder.SeedAsync(assetResult, collaborationResult, ioTResult);
    await collaborationSeeder.BindWorkOrderAsync(collaborationResult.EventId, workOrderResult.WorkOrderId, workOrderResult.WorkOrderNumber);
    await assetSeeder.BindWorkOrderAsync(assetResult.WalkwayAssetId, workOrderResult.WorkOrderId);
}

Console.WriteLine("SmartPark demo data seeding completed.");

static HashSet<SeederModule> ParseModules(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return Enum.GetValues<SeederModule>().ToHashSet();
    }

    var modules = new HashSet<SeederModule>();
    var tokens = value.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    foreach (var token in tokens)
    {
        if (token.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            return Enum.GetValues<SeederModule>().ToHashSet();
        }

        if (!TryParseModule(token, out var module))
        {
            throw new InvalidOperationException($"Unsupported module '{token}'. Supported values: all, identity, asset, iotplatform, collaboration, workorder.");
        }

        modules.Add(module);
    }

    return modules.Count == 0 ? Enum.GetValues<SeederModule>().ToHashSet() : modules;
}

static bool TryParseModule(string token, out SeederModule module)
{
    if (token.Equals("iot", StringComparison.OrdinalIgnoreCase))
    {
        module = SeederModule.IoTPlatform;
        return true;
    }

    return Enum.TryParse(token, true, out module);
}

static HashSet<SeederModule> ExpandModules(HashSet<SeederModule> modules)
{
    if (modules.Contains(SeederModule.WorkOrder))
    {
        modules.Add(SeederModule.Collaboration);
    }

    if (modules.Contains(SeederModule.Collaboration))
    {
        modules.Add(SeederModule.IoTPlatform);
    }

    if (modules.Contains(SeederModule.IoTPlatform))
    {
        modules.Add(SeederModule.Asset);
    }

    return modules;
}

file sealed class SeederCurrentUser : ICurrentUser
{
    public string? UserId => "db-seeder";

    public string? UserName => "db-seeder";

    public string? DisplayName => "db-seeder";

    public IReadOnlyCollection<string> Roles => Array.Empty<string>();

    public bool IsAuthenticated => false;
}
