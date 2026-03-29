namespace ClearEyeQ.Notifications.Domain.ValueObjects;

public sealed record QuietHoursPolicy(
    TimeOnly Start,
    TimeOnly End,
    string TimeZone)
{
    public bool IsInQuietHours(DateTimeOffset utcNow)
    {
        try
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
            var localTime = TimeZoneInfo.ConvertTime(utcNow, timeZoneInfo);
            var localTimeOnly = TimeOnly.FromDateTime(localTime.DateTime);

            if (Start <= End)
            {
                return localTimeOnly >= Start && localTimeOnly <= End;
            }

            // Quiet hours span midnight (e.g., 22:00 - 07:00)
            return localTimeOnly >= Start || localTimeOnly <= End;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
    }
}
