using ClearEyeQ.Clinical.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.Events;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Clinical.Application.Commands.AcceptReferral;

public sealed class AcceptReferralHandler : IRequestHandler<AcceptReferralCommand, Unit>
{
    private readonly IReferralRepository _referralRepository;
    private readonly IIntegrationEventPublisher _eventPublisher;

    public AcceptReferralHandler(
        IReferralRepository referralRepository,
        IIntegrationEventPublisher eventPublisher)
    {
        _referralRepository = referralRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<Unit> Handle(AcceptReferralCommand request, CancellationToken cancellationToken)
    {
        await _referralRepository.UpdateStatusAsync(
            request.ReferralId,
            "Accepted",
            request.ClinicianId,
            request.Rationale,
            cancellationToken);

        var payload = new ReferralAcceptedPayload(
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

internal sealed record ReferralAcceptedPayload(
    Guid ReferralId,
    string ClinicianId,
    string Rationale,
    DateTimeOffset AcceptedAtUtc);
