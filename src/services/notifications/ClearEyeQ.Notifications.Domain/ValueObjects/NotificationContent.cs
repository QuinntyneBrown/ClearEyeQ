namespace ClearEyeQ.Notifications.Domain.ValueObjects;

public sealed record NotificationContent(
    string Title,
    string Body,
    string? ActionUrl,
    Dictionary<string, string> Data)
{
    public static NotificationContent Create(string title, string body, string? actionUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        return new NotificationContent(title, body, actionUrl, new Dictionary<string, string>());
    }
}
