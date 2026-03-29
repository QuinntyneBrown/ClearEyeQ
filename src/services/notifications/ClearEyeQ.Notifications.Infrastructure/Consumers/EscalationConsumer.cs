using ClearEyeQ.Notifications.Application.Commands.SendNotification;
using ClearEyeQ.Notifications.Domain.Enums;
using ClearEyeQ.SharedKernel.Infrastructure.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ClearEyeQ.Notifications.Infrastructure.Consumers;

public sealed record EscalationMessage(
    Guid UserId,
    Guid TenantId,
    Guid PlanId,
    string RecommendedAction);

public sealed class EscalationConsumer : InboxConsumer<EscalationMessage>
{
    private readonly IMediator _mediator;

    public EscalationConsumer(
        IConnectionMultiplexer redis,
        IMediator mediator,
        ILogger<EscalationConsumer> logger)
        : base(redis, logger)
    {
        _mediator = mediator;
    }

    protected override async Task HandleAsync(EscalationMessage message, CancellationToken ct)
    {
        await _mediator.Send(new SendNotificationCommand(
            UserId: message.UserId,
            TenantId: message.TenantId,
            Category: NotificationCategory.SpecialistReferral,
            Title: "Treatment Escalation Recommended",
            Body: $"Your treatment plan requires attention. Recommended action: {message.RecommendedAction}",
            ActionUrl: $"/treatment/plans/{message.PlanId}",
            PreferredChannel: NotificationChannel.Push), ct);
    }
}
