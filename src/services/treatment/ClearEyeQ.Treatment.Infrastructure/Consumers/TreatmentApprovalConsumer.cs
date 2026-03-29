using ClearEyeQ.SharedKernel.Infrastructure.Messaging;
using ClearEyeQ.Treatment.Application.Commands.ActivateTreatmentPlan;
using ClearEyeQ.Treatment.Application.Commands.RejectTreatmentPlan;
using MediatR;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ClearEyeQ.Treatment.Infrastructure.Consumers;

public sealed record TreatmentApprovalMessage(
    Guid PlanId,
    Guid TenantId,
    bool Approved,
    string? RejectionReason);

public sealed class TreatmentApprovalConsumer : InboxConsumer<TreatmentApprovalMessage>
{
    private readonly IMediator _mediator;

    public TreatmentApprovalConsumer(
        IConnectionMultiplexer redis,
        IMediator mediator,
        ILogger<TreatmentApprovalConsumer> logger)
        : base(redis, logger)
    {
        _mediator = mediator;
    }

    protected override async Task HandleAsync(TreatmentApprovalMessage message, CancellationToken ct)
    {
        if (message.Approved)
        {
            await _mediator.Send(new ActivateTreatmentPlanCommand(
                message.PlanId,
                message.TenantId), ct);
        }
        else
        {
            await _mediator.Send(new RejectTreatmentPlanCommand(
                message.PlanId,
                message.TenantId,
                message.RejectionReason ?? "Rejected by clinician"), ct);
        }
    }
}
