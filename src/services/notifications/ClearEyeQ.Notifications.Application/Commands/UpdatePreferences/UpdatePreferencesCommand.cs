using ClearEyeQ.Notifications.Domain.Enums;
using MediatR;

namespace ClearEyeQ.Notifications.Application.Commands.UpdatePreferences;

public sealed record UpdatePreferencesCommand(
    Guid UserId,
    Guid TenantId,
    NotificationChannel Channel,
    bool Enabled,
    TimeOnly? QuietHoursStart = null,
    TimeOnly? QuietHoursEnd = null,
    string? TimeZone = null) : IRequest;
