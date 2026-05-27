using Microsoft.EntityFrameworkCore;
using SmartPark.IoTPlatform.Domain;

namespace SmartPark.IoTPlatform.Infrastructure;

public sealed class IoTDatabaseInitializer(IoTPlatformDbContext dbContext)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (!await dbContext.MonitoringPoints.AnyAsync(cancellationToken))
        {
            dbContext.MonitoringPoints.AddRange(
                new MonitoringPoint
                {
                    Name = "东门土壤墒情点",
                    MetricType = MonitoringMetricType.SoilMoisture,
                    Unit = "%",
                    CurrentValue = 35.6m,
                    Area = "东门片区",
                    StatusText = "Normal"
                },
                new MonitoringPoint
                {
                    Name = "中心湖水位点",
                    MetricType = MonitoringMetricType.WaterLevel,
                    Unit = "m",
                    CurrentValue = 1.2m,
                    Area = "中心湖区",
                    StatusText = "Warning"
                });
        }

        if (!await dbContext.Alerts.AnyAsync(cancellationToken))
        {
            dbContext.Alerts.Add(new AlertRecord
            {
                Title = "中心湖水位预警",
                Message = "受连续降雨影响，中心湖水位接近预警阈值。",
                Level = AlertLevel.Warning,
                Status = AlertStatus.Open
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
