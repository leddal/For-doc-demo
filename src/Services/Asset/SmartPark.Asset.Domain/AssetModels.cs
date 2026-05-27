using SmartPark.SharedKernel;

namespace SmartPark.Asset.Domain;

/// <summary>
/// 资产类型。
/// 统一覆盖设备、基础设施和植物三类台账对象。
/// </summary>
public enum AssetType
{
    Device = 1,
    Infrastructure = 2,
    Plant = 3
}

/// <summary>
/// 资产状态。
/// </summary>
public enum AssetStatus
{
    Active = 1,
    Maintenance = 2,
    Offline = 3
}

/// <summary>
/// 公园资产聚合根。
/// 用于承接工单、事件、监测点等对象与资产台账的关联关系。
/// </summary>
public sealed class ParkAsset : AuditableEntity
{
    public string AssetCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public AssetType Type { get; set; }

    public AssetStatus Status { get; set; } = AssetStatus.Active;

    public string Area { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string? Model { get; set; }

    public string? Description { get; set; }

    // 当前若有在途工单，可在这里保留关联，方便前端快速定位处理对象。
    public Guid? CurrentWorkOrderId { get; set; }
}
