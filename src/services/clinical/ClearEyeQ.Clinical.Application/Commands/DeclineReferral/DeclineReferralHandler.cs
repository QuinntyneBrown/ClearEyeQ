using ClearEyeQ.Clinical.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.Events;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Clinical.Application.Commands.DeclineReferral;

public sealed class DeclineReferralHandler : IRequestHandler<DeclineReferralCommand, Unit>
{
    private readonly IReferralRepository _referralRepository;
    private readonly IIntegrationEventPublisher _eventPublisher;

    public DeclineReferralHandler(
        IReferralRepository referralRepository,
        IIntegrationEventPublisher eventPublisher)
    {
        _referralRepository = referralRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<Unit> Handle(DeclineReferralCommand request, CancellationToken cancellationToken)
    {
        await _referralRepository.UpdateStatusAsync(
            request.ReferralId,
            "Declined",
            request.ClinicianId,
            request.Rationale,
            cancellationToken);

        var payload = new ReferralDeclinedPayload(
            request.ReferralId,
            request.ClinicianId,
            request.Rationale,
            DateTimeOffset.UtcNow);

        var envelope = IntegrationEventEnvelope.Create(
            payload,
            new TenantId(request.TenantId),
            request.ReferralId,
            Guid.NewGuid(),
            Guid.NewGuid());

        await _eventPublisher.PublishAsync(envelope, cancellationToken);

        return Unit.Value;
    }
}

internal sealed record ReferralDeclinedPayload(
    Guid ReferralId,
    string ClinicianId,
    string Rationale,
    DateTimeOffset DeclinedAtUtc);
