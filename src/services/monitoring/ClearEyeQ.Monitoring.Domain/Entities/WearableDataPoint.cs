using ClearEyeQ.Monitoring.Domain.Enums;

namespace ClearEyeQ.Monitoring.Domain.Entities;

public sealed class WearableDataPoint
{
    public Guid Id { get; private set; }
    public WearableSource Source { get; private set; }
    public MetricType MetricType { get; private set; }
    public double Value { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }

    private WearableDataPoint() { }

    public static WearableDataPoint Create(
        WearableSource source,
        MetricType metricType,
        double value,
        DateTimeOffset timestamp)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("Value must be a finite number.", nameof(value));

        return new WearableDataPoint
        {
            Id = Guid.NewGuid(),
            Source = source,
            MetricType = metricType,
            Value = value,
            Timestamp = timestamp
        };
    }
}
