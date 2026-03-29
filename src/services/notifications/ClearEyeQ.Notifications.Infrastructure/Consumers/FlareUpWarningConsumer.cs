using ClearEyeQ.Notifications.Application.Commands.SendNotification;
using ClearEyeQ.Notifications.Domain.Enums;
using ClearEyeQ.SharedKernel.Infrastructure.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ClearEyeQ.Notifications.Infrastructure.Consumers;

public sealed record FlareUpWarningMessage(
    Guid UserId,
    Guid TenantId,
    string Severity,
    string Description);

public sealed class FlareUpWarningConsumer : InboxConsumer<FlareUpWarningMessage>
{
    private readonly IMediator _mediator;

    public FlareUpWarningConsumer(
        IConnectionMultiplexer redis,
        IMediator mediator,
        ILogger<FlareUpWarningConsumer> logger)
        : base(redis, logger)
    {
        _mediator = mediator;
    }

    protected override async Task HandleAsync(FlareUpWarningMessage message, CancellationToken ct)
    {
        await _mediator.Send(new SendNotificationCommand(
            UserId: message.UserId,
            TenantId: message.TenantId,
            Category: NotificationCategory.FlareUpAlert,
            Title: "Flare-Up Warning",
            Body: $"A potential flare-up has been detected ({message.Severity}): {message.Description}",
            ActionUrl: "/dashboard/alerts",
            PreferredChannel: NotificationChannel.Push), ct);
    }
}
