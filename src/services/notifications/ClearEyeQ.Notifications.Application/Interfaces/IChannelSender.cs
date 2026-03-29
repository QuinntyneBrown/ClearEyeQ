using ClearEyeQ.Notifications.Domain.Enums;
using ClearEyeQ.Notifications.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Notifications.Application.Interfaces;

public interface IChannelSender
{
    NotificationChannel Channel { get; }
    Task<bool> SendAsync(UserId userId, NotificationContent content, CancellationToken ct);
}
