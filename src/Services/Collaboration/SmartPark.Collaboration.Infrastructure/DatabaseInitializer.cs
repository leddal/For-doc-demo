using Microsoft.EntityFrameworkCore;
using SmartPark.Collaboration.Domain;

namespace SmartPark.Collaboration.Infrastructure;

public sealed class CollaborationDatabaseInitializer(CollaborationDbContext dbContext)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (!await dbContext.Announcements.AnyAsync(cancellationToken))
        {
            dbContext.Announcements.Add(new Announcement
            {
                Title = "园区活动公告",
                Content = "本周末东门区域将举行公益导览活动，请游客合理安排路线。",
                IsPublished = true,
                PublishedAt = DateTimeOffset.UtcNow
            });
        }

        if (!await dbContext.Events.AnyAsync(cancellationToken))
        {
            dbContext.Events.Add(new EventRecord
            {
                Code = "EVT-DEMO-001",
                Title = "东门区域积水预警",
                Description = "连续降雨导致东门步道存在积水风险，需要巡检处理。",
                Severity = EventSeverity.High,
                Area = "东门片区"
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
