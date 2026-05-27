namespace SmartPark.SharedInfrastructure.Abstractions;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
