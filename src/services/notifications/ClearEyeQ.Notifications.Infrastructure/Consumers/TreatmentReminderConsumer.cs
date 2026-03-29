using ClearEyeQ.Notifications.Application.Commands.SendNotification;
using ClearEyeQ.Notifications.Domain.Enums;
using ClearEyeQ.SharedKernel.Infrastructure.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ClearEyeQ.Notifications.Infrastructure.Consumers;

public sealed record TreatmentReminderMessage(
    Guid UserId,
    Guid TenantId,
    string TreatmentName,
    string ReminderText);

public sealed class TreatmentReminderConsumer : InboxConsumer<TreatmentReminderMessage>
{
    private readonly IMediator _mediator;

    public TreatmentReminderConsumer(
        IConnectionMultiplexer redis,
        IMediator mediator,
        ILogger<TreatmentReminderConsumer> logger)
        : base(redis, logger)
    {
        _mediator = mediator;
    }

    protected override async Task HandleAsync(TreatmentReminderMessage message, CancellationToken ct)
    {
        await _mediator.Send(new SendNotificationCommand(
            UserId: message.UserId,
            TenantId: message.TenantId,
            Category: NotificationCategory.TreatmentReminder,
            Title: "Treatment Reminder",
            Body: $"{message.TreatmentName}: {message.ReminderText}",
            ActionUrl: "/treatment/current",
            PreferredChannel: NotificationChannel.Push), ct);
    }
}
