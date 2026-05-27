namespace SmartPark.SharedKernel;

/// <summary>
/// 带审计字段的实体基类。
/// 适合需要记录创建时间、创建人、修改时间和修改人的业务对象。
/// </summary>
public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }
}
