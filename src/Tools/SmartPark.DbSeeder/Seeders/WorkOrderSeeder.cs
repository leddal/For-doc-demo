using Microsoft.EntityFrameworkCore;
using SmartPark.DbSeeder.Options;
using SmartPark.WorkOrder.Domain;
using SmartPark.WorkOrder.Infrastructure;
using WorkOrderEntity = SmartPark.WorkOrder.Domain.WorkOrder;

namespace SmartPark.DbSeeder.Seeders;

public sealed class WorkOrderSeeder(WorkOrderDbContext dbContext)
{
    public async Task<WorkOrderSeedResult> SeedAsync(
        AssetSeedResult assets,
        CollaborationSeedResult collaboration,
        IoTSeedResult ioT,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        var workOrder = await dbContext.WorkOrders
            .Include(item => item.Attachments)
            .FirstOrDefaultAsync(item => item.Number == DemoSeedData.WorkOrderNumber, cancellationToken);

        if (workOrder is null)
        {
            workOrder = WorkOrderEntity.Create(
                DemoSeedData.WorkOrderNumber,
                "示例园区巡检工单",
                "对东门区域积水告警进行现场巡检和处理。",
                WorkOrderSourceType.Event,
                WorkOrderBusinessType.Inspection,
                WorkOrderPriority.High,
                "东门片区",
                assets.WalkwayAssetId,
                collaboration.EventId,
                ioT.WaterLevelAlertId,
                "系统种子数据",
                "seed",
                "系统初始化",
                DateTimeOffset.UtcNow);

            dbContext.WorkOrders.Add(workOrder);
        }

        workOrder.Title = "示例园区巡检工单";
        workOrder.Description = "对东门区域积水告警进行现场巡检和处理。";
        workOrder.SourceType = WorkOrderSourceType.Event;
        workOrder.BusinessType = WorkOrderBusinessType.Inspection;
        workOrder.Priority = WorkOrderPriority.High;
        workOrder.ParkArea = "东门片区";
        workOrder.RelatedAssetId = assets.WalkwayAssetId;
        workOrder.RelatedEventId = collaboration.EventId;
        workOrder.RelatedAlertId = ioT.WaterLevelAlertId;
        workOrder.ReporterName = "系统种子数据";

        if (!workOrder.Attachments.Any(item => item.FileName == DemoSeedData.WorkOrderAttachmentFileName && item.Url == DemoSeedData.WorkOrderAttachmentUrl))
        {
            workOrder.AddAttachment(
                DemoSeedData.WorkOrderAttachmentFileName,
                DemoSeedData.WorkOrderAttachmentUrl,
                DemoSeedData.WorkOrderAttachmentContentType);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new WorkOrderSeedResult(workOrder.Id, workOrder.Number);
    }
}

public sealed record WorkOrderSeedResult(Guid WorkOrderId, string WorkOrderNumber);
