using SmartPark.SharedInfrastructure.Abstractions;

namespace SmartPark.SharedInfrastructure.Runtime;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
