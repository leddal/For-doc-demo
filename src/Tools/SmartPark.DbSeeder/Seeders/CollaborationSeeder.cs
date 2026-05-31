using Microsoft.EntityFrameworkCore;
using SmartPark.Collaboration.Domain;
using SmartPark.Collaboration.Infrastructure;
using SmartPark.DbSeeder.Options;

namespace SmartPark.DbSeeder.Seeders;

public sealed class CollaborationSeeder(CollaborationDbContext dbContext)
{
    public async Task<CollaborationSeedResult> SeedAsync(AssetSeedResult assets, IoTSeedResult ioT, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        var announcement = await dbContext.Announcements.FirstOrDefaultAsync(item => item.Title == DemoSeedData.AnnouncementTitle, cancellationToken);
        if (announcement is null)
        {
            announcement = new Announcement
            {
                Title = DemoSeedData.AnnouncementTitle,
                PublishedAt = DateTimeOffset.UtcNow
            };

            dbContext.Announcements.Add(announcement);
        }

        announcement.Content = "本周末东门区域将举行公益导览活动，请游客合理安排路线。";
        announcement.IsPublished = true;
        if (announcement.PublishedAt == default)
        {
            announcement.PublishedAt = DateTimeOffset.UtcNow;
        }

        var eventRecord = await dbContext.Events.FirstOrDefaultAsync(item => item.Code == DemoSeedData.EventCode, cancellationToken);
        if (eventRecord is null)
        {
            eventRecord = new EventRecord
            {
                Code = DemoSeedData.EventCode
            };

            dbContext.Events.Add(eventRecord);
        }

        eventRecord.Title = "东门区域积水预警";
        eventRecord.Description = "连续降雨导致东门步道存在积水风险，需要巡检处理。";
        eventRecord.Severity = EventSeverity.High;
        eventRecord.Area = "东门片区";
        eventRecord.RelatedAssetId = assets.WalkwayAssetId;
        eventRecord.RelatedAlertId = ioT.WaterLevelAlertId;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CollaborationSeedResult(eventRecord.Id);
    }

    public async Task BindWorkOrderAsync(Guid eventId, Guid workOrderId, string workOrderNumber, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        var eventRecord = await dbContext.Events.FirstOrDefaultAsync(item => item.Id == eventId, cancellationToken);
        if (eventRecord is null || eventRecord.Status == EventStatus.Closed)
        {
            return;
        }

        if (eventRecord.WorkOrderId == workOrderId && string.Equals(eventRecord.WorkOrderNumber, workOrderNumber, StringComparison.Ordinal))
        {
            return;
        }

        if (eventRecord.WorkOrderId is not null && eventRecord.WorkOrderId != workOrderId)
        {
            return;
        }

        eventRecord.BindWorkOrder(workOrderId, workOrderNumber);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed record CollaborationSeedResult(Guid EventId);
