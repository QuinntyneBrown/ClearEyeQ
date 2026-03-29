using ClearEyeQ.Clinical.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.Events;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Clinical.Application.Commands.RejectTreatmentAdjustment;

public sealed class RejectTreatmentAdjustmentHandler : IRequestHandler<RejectTreatmentAdjustmentCommand, Unit>
{
    private readonly IIntegrationEventPublisher _eventPublisher;

    public RejectTreatmentAdjustmentHandler(IIntegrationEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public async Task<Unit> Handle(RejectTreatmentAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var payload = new TreatmentAdjustmentRejectedPayload(
            request.TreatmentReviewId,
            request.ClinicianId,
            request.Rationale,
            DateTimeOffset.UtcNow);

        var envelope = IntegrationEventEnvelope.Create(
            payload,
            new TenantId(request.TenantId),
            request.TreatmentReviewId,
            Guid.NewGuid(),
            Guid.NewGuid());

        await _eventPublisher.PublishAsync(envelope, cancellationToken);

        return Unit.Value;
    }
}

internal sealed record TreatmentAdjustmentRejectedPayload(
    Guid TreatmentReviewId,
    string ClinicianId,
    string Rationale,
    DateTimeOffset RejectedAtUtc);
