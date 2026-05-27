using SmartPark.SharedKernel;

namespace SmartPark.Identity.Domain;

/// <summary>
/// 系统内置角色编码。
/// 角色用于做粗粒度访问控制。
/// </summary>
public static class RoleCodes
{
    public const string Admin = "Admin";
    public const string Dispatcher = "Dispatcher";
    public const string Operator = "Operator";
    public const string Reviewer = "Reviewer";

    public static readonly IReadOnlyCollection<string> All = [Admin, Dispatcher, Operator, Reviewer];
}

/// <summary>
/// 系统权限码定义。
/// 权限码既可用于前端按钮显示控制，也可用于后续扩展细粒度授权。
/// </summary>
public static class PermissionCodes
{
    public const string DashboardView = "dashboard:view";
    public const string WorkOrderDispatch = "workorder:dispatch";
    public const string WorkOrderProcess = "workorder:process";
    public const string WorkOrderVerify = "workorder:verify";
    public const string AssetManage = "asset:manage";
    public const string EventManage = "event:manage";
    public const string AnnouncementManage = "announcement:manage";
    public const string UserManage = "user:manage";
}

/// <summary>
/// 角色与权限的静态映射表。
/// 当前项目为了便于学习和演示，先使用代码内置方式维护权限矩阵。
/// </summary>
public static class RolePermissionMatrix
{
    public static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Definitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [RoleCodes.Admin] =
            [
                PermissionCodes.DashboardView,
                PermissionCodes.WorkOrderDispatch,
                PermissionCodes.WorkOrderProcess,
                PermissionCodes.WorkOrderVerify,
                PermissionCodes.AssetManage,
                PermissionCodes.EventManage,
                PermissionCodes.AnnouncementManage,
                PermissionCodes.UserManage
            ],
            [RoleCodes.Dispatcher] =
            [
                PermissionCodes.DashboardView,
                PermissionCodes.WorkOrderDispatch,
                PermissionCodes.EventManage,
                PermissionCodes.AnnouncementManage
            ],
            [RoleCodes.Operator] =
            [
                PermissionCodes.DashboardView,
                PermissionCodes.WorkOrderProcess,
                PermissionCodes.AssetManage
            ],
            [RoleCodes.Reviewer] =
            [
                PermissionCodes.DashboardView,
                PermissionCodes.WorkOrderVerify,
                PermissionCodes.EventManage
            ]
        };
}

/// <summary>
/// 系统用户实体。
/// 承载用户名、显示名、邮箱、密码散列和激活状态。
/// </summary>
public sealed class User : AuditableEntity
{
    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

/// <summary>
/// 角色实体。
/// 通过与 UserRole 关联形成用户到角色的多对多关系。
/// </summary>
public sealed class Role : AuditableEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

/// <summary>
/// 用户角色关联实体。
/// </summary>
public sealed class UserRole
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }

    public Role Role { get; set; } = null!;
}
