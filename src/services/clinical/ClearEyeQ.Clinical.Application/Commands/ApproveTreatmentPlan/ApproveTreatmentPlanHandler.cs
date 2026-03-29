using ClearEyeQ.Clinical.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.Events;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Clinical.Application.Commands.ApproveTreatmentPlan;

public sealed class ApproveTreatmentPlanHandler : IRequestHandler<ApproveTreatmentPlanCommand, Unit>
{
    private readonly IIntegrationEventPublisher _eventPublisher;

    public ApproveTreatmentPlanHandler(IIntegrationEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public async Task<Unit> Handle(ApproveTreatmentPlanCommand request, CancellationToken cancellationToken)
    {
        var payload = new TreatmentPlanApprovedPayload(
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

internal sealed record TreatmentPlanApprovedPayload(
    Guid TreatmentReviewId,
    string ClinicianId,
    string Rationale,
    DateTimeOffset ApprovedAtUtc);
