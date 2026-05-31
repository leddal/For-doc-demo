using Microsoft.Extensions.Configuration;

namespace SmartPark.DbSeeder.Options;

public enum SeederModule
{
    Identity,
    Asset,
    IoTPlatform,
    Collaboration,
    WorkOrder
}

public sealed class SeederOptions
{
    private const string DefaultIdentityConnectionString = "Host=localhost;Port=5432;Database=smartpark_identity;Username=postgres;Password=postgres";
    private const string DefaultAssetConnectionString = "Host=localhost;Port=5432;Database=smartpark_asset;Username=postgres;Password=postgres";
    private const string DefaultIoTConnectionString = "Host=localhost;Port=5432;Database=smartpark_iot;Username=postgres;Password=postgres";
    private const string DefaultCollaborationConnectionString = "Host=localhost;Port=5432;Database=smartpark_collaboration;Username=postgres;Password=postgres";
    private const string DefaultWorkOrderConnectionString = "Host=localhost;Port=5432;Database=smartpark_workorder;Username=postgres;Password=postgres";

    public string IdentityConnectionString { get; init; } = DefaultIdentityConnectionString;

    public string AssetConnectionString { get; init; } = DefaultAssetConnectionString;

    public string IoTConnectionString { get; init; } = DefaultIoTConnectionString;

    public string CollaborationConnectionString { get; init; } = DefaultCollaborationConnectionString;

    public string WorkOrderConnectionString { get; init; } = DefaultWorkOrderConnectionString;

    public static SeederOptions FromConfiguration(IConfiguration configuration)
    {
        return new SeederOptions
        {
            IdentityConnectionString = configuration["ConnectionStrings:IdentityPostgres"] ?? DefaultIdentityConnectionString,
            AssetConnectionString = configuration["ConnectionStrings:AssetPostgres"] ?? DefaultAssetConnectionString,
            IoTConnectionString = configuration["ConnectionStrings:IoTPostgres"] ?? DefaultIoTConnectionString,
            CollaborationConnectionString = configuration["ConnectionStrings:CollaborationPostgres"] ?? DefaultCollaborationConnectionString,
            WorkOrderConnectionString = configuration["ConnectionStrings:WorkOrderPostgres"] ?? DefaultWorkOrderConnectionString
        };
    }
}

public static class DemoSeedData
{
    public const string DefaultPassword = "SmartPark@123";

    public const string AdminUserName = "admin";
    public const string DispatcherUserName = "dispatcher";
    public const string OperatorUserName = "operator";
    public const string ReviewerUserName = "reviewer";

    public const string CameraAssetCode = "DEV-001";
    public const string WalkwayAssetCode = "INF-001";
    public const string GinkgoAssetCode = "PLT-001";

    public const string SoilMonitoringPointName = "东门土壤墒情点";
    public const string WaterLevelMonitoringPointName = "中心湖水位点";
    public const string WaterLevelAlertTitle = "中心湖水位预警";

    public const string AnnouncementTitle = "园区活动公告";
    public const string EventCode = "EVT-DEMO-001";
    public const string WorkOrderNumber = "WO-DEMO-001";

    public const string WorkOrderAttachmentFileName = "现场照片.jpg";
    public const string WorkOrderAttachmentUrl = "/files/demo-workorder-photo.jpg";
    public const string WorkOrderAttachmentContentType = "image/jpeg";
}
