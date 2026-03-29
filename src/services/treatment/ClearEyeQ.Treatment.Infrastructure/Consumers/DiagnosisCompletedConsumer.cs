using ClearEyeQ.SharedKernel.Infrastructure.Messaging;
using ClearEyeQ.Treatment.Application.Commands.CreateTreatmentPlan;
using MediatR;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ClearEyeQ.Treatment.Infrastructure.Consumers;

public sealed record DiagnosisCompletedMessage(
    Guid DiagnosisId,
    Guid UserId,
    Guid TenantId,
    string Severity);

public sealed class DiagnosisCompletedConsumer : InboxConsumer<DiagnosisCompletedMessage>
{
    private readonly IMediator _mediator;

    public DiagnosisCompletedConsumer(
        IConnectionMultiplexer redis,
        IMediator mediator,
        ILogger<DiagnosisCompletedConsumer> logger)
        : base(redis, logger)
    {
        _mediator = mediator;
    }

    protected override async Task HandleAsync(DiagnosisCompletedMessage message, CancellationToken ct)
    {
        var escalationDays = message.Severity switch
        {
            "Critical" or "Severe" => 7,
            "Moderate" => 14,
            _ => 30
        };

        var command = new CreateTreatmentPlanCommand(
            UserId: message.UserId,
            TenantId: message.TenantId,
            DiagnosisId: message.DiagnosisId,
            EscalationDaysThreshold: escalationDays,
            EscalationMinImprovementPercent: 20.0,
            EscalationAction: "Specialist referral recommended");

        await _mediator.Send(command, ct);
    }
}
