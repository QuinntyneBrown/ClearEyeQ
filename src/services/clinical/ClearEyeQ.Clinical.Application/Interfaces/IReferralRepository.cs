using ClearEyeQ.Clinical.Application.Queries.GetReferralInbox;

namespace ClearEyeQ.Clinical.Application.Interfaces;

/// <summary>
/// Repository for managing referral cases in the clinical portal.
/// </summary>
public interface IReferralRepository
{
    Task<IReadOnlyList<ReferralDto>> GetPendingReferralsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<ReferralDto?> GetByIdAsync(Guid tenantId, Guid referralId, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid referralId, string status, string clinicianId, string rationale, CancellationToken cancellationToken = default);
    Task CreateAsync(Guid id, Guid tenantId, Guid patientId, string patientName, string reason, string severity, CancellationToken cancellationToken = default);
}
