using ClearEyeQ.Clinical.Application.Interfaces;
using ClearEyeQ.SharedKernel.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ClearEyeQ.Clinical.Infrastructure.Projectors;

/// <summary>
/// Subscribes to EscalationRecommended integration events and creates
/// referral cases for clinician review in the referral inbox.
/// </summary>
public sealed class EscalationProjector : InboxConsumer<EscalationProjector.EscalationRecommendedEvent>
{
    private readonly IReferralRepository _referralRepository;

    public EscalationProjector(
        IReferralRepository referralRepository,
        IConnectionMultiplexer redis,
        ILogger<EscalationProjector> logger)
        : base(redis, logger)
    {
        _referralRepository = referralRepository;
    }

    protected override async Task HandleAsync(EscalationRecommendedEvent message, CancellationToken ct)
    {
        await _referralRepository.CreateAsync(
            Guid.NewGuid(),
            message.TenantId,
            message.PatientId,
            message.PatientName,
            message.Reason,
            message.Severity,
            ct);
    }

    public sealed record EscalationRecommendedEvent(
        Guid TenantId,
        Guid PatientId,
        string PatientName,
        string Reason,
        string Severity);
}
