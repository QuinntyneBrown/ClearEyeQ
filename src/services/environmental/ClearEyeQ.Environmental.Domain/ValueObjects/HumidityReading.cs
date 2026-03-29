namespace ClearEyeQ.Environmental.Domain.ValueObjects;

public sealed record HumidityReading(double Percentage)
{
    public bool IsComfortableForEyes => Percentage is >= 30 and <= 60;
}
