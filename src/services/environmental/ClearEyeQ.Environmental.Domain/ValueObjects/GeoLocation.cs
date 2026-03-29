namespace ClearEyeQ.Environmental.Domain.ValueObjects;

public sealed record GeoLocation(double Latitude, double Longitude)
{
    public void Validate()
    {
        if (Latitude is < -90.0 or > 90.0)
            throw new ArgumentOutOfRangeException(nameof(Latitude), "Latitude must be between -90 and 90.");
        if (Longitude is < -180.0 or > 180.0)
            throw new ArgumentOutOfRangeException(nameof(Longitude), "Longitude must be between -180 and 180.");
    }
}
