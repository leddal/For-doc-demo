using Microsoft.EntityFrameworkCore;
using SmartPark.DbSeeder.Options;
using SmartPark.IoTPlatform.Domain;
using SmartPark.IoTPlatform.Infrastructure;

namespace SmartPark.DbSeeder.Seeders;

public sealed class IoTSeeder(IoTPlatformDbContext dbContext)
{
    public async Task<IoTSeedResult> SeedAsync(AssetSeedResult assets, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        var pointSpecs = new[]
        {
            new MonitoringPointSeedSpec(DemoSeedData.SoilMonitoringPointName, MonitoringMetricType.SoilMoisture, "%", 35.6m, "东门片区", "Normal"),
            new MonitoringPointSeedSpec(DemoSeedData.WaterLevelMonitoringPointName, MonitoringMetricType.WaterLevel, "m", 1.2m, "中心湖区", "Warning")
        };

        var pointNames = pointSpecs.Select(item => item.Name).ToArray();
        var points = await dbContext.MonitoringPoints
            .Where(item => pointNames.Contains(item.Name))
            .ToDictionaryAsync(item => item.Name, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var spec in pointSpecs)
        {
            if (!points.TryGetValue(spec.Name, out var point))
            {
                point = new MonitoringPoint
                {
                    Name = spec.Name
                };

                dbContext.MonitoringPoints.Add(point);
                points[spec.Name] = point;
            }

            point.MetricType = spec.MetricType;
            point.Unit = spec.Unit;
            point.CurrentValue = spec.CurrentValue;
            point.Area = spec.Area;
            point.StatusText = spec.StatusText;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var alert = await dbContext.Alerts.FirstOrDefaultAsync(item => item.Title == DemoSeedData.WaterLevelAlertTitle, cancellationToken);
        if (alert is null)
        {
            alert = new AlertRecord
            {
                Title = DemoSeedData.WaterLevelAlertTitle
            };

            dbContext.Alerts.Add(alert);
        }

        alert.Message = "受连续降雨影响，中心湖水位接近预警阈值。";
        alert.Level = AlertLevel.Warning;
        alert.Status = AlertStatus.Open;
        alert.RelatedAssetId = assets.WalkwayAssetId;
        alert.MonitoringPointId = points[DemoSeedData.WaterLevelMonitoringPointName].Id;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new IoTSeedResult(
            points[DemoSeedData.SoilMonitoringPointName].Id,
            points[DemoSeedData.WaterLevelMonitoringPointName].Id,
            alert.Id);
    }
}

public sealed record IoTSeedResult(Guid SoilMonitoringPointId, Guid WaterLevelMonitoringPointId, Guid WaterLevelAlertId);

file sealed record MonitoringPointSeedSpec(
    string Name,
    MonitoringMetricType MetricType,
    string Unit,
    decimal CurrentValue,
    string Area,
    string StatusText);
