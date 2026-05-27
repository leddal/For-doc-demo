using Microsoft.EntityFrameworkCore;
using SmartPark.WorkOrder.Domain;

namespace SmartPark.WorkOrder.Infrastructure;

public sealed class WorkOrderDatabaseInitializer(WorkOrderDbContext dbContext)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await dbContext.WorkOrders.AnyAsync(cancellationToken))
        {
            return;
        }

        var workOrder = Domain.WorkOrder.Create(
            "WO-DEMO-001",
            "示例园区巡检工单",
            "对东门区域积水告警进行现场巡检和处理。",
            WorkOrderSourceType.Event,
            WorkOrderBusinessType.Inspection,
            WorkOrderPriority.High,
            "东门片区",
            null,
            Guid.NewGuid(),
            null,
            "系统种子数据",
            "seed",
            "系统初始化",
            DateTimeOffset.UtcNow);

        workOrder.AddAttachment("现场照片.jpg", "/files/demo-workorder-photo.jpg", "image/jpeg");

        dbContext.WorkOrders.Add(workOrder);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
