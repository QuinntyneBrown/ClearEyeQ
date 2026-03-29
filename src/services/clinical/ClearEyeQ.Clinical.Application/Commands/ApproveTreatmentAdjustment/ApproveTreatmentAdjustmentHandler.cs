using ClearEyeQ.Clinical.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.Events;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Clinical.Application.Commands.ApproveTreatmentAdjustment;

public sealed class ApproveTreatmentAdjustmentHandler : IRequestHandler<ApproveTreatmentAdjustmentCommand, Unit>
{
    private readonly IIntegrationEventPublisher _eventPublisher;

    public ApproveTreatmentAdjustmentHandler(IIntegrationEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public async Task<Unit> Handle(ApproveTreatmentAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var payload = new TreatmentAdjustmentApprovedPayload(
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

internal sealed record TreatmentAdjustmentApprovedPayload(
    Guid TreatmentReviewId,
    string ClinicianId,
    string Rationale,
    DateTimeOffset ApprovedAtUtc);
