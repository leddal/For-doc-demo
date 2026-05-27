namespace SmartPark.SharedKernel;

/// <summary>
/// 所有实体对象的基础父类。
/// 统一提供 Guid 主键，避免各服务重复定义主键字段。
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
