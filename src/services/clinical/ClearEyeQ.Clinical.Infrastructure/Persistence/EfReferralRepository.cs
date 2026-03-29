using ClearEyeQ.Clinical.Application.Interfaces;
using ClearEyeQ.Clinical.Application.Queries.GetReferralInbox;
using Microsoft.EntityFrameworkCore;

namespace ClearEyeQ.Clinical.Infrastructure.Persistence;

public sealed class EfReferralRepository : IReferralRepository
{
    private readonly ClinicalDbContext _db;

    public EfReferralRepository(ClinicalDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ReferralDto>> GetPendingReferralsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _db.Referrals
            .Where(r => r.TenantId == tenantId && r.Status == "Pending")
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new ReferralDto(
                r.Id,
                r.PatientId,
                r.PatientName,
                r.Reason,
                r.Severity,
                r.Status,
                r.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<ReferralDto?> GetByIdAsync(Guid tenantId, Guid referralId, CancellationToken cancellationToken = default)
    {
        return await _db.Referrals
            .Where(r => r.TenantId == tenantId && r.Id == referralId)
            .Select(r => new ReferralDto(
                r.Id,
                r.PatientId,
                r.PatientName,
                r.Reason,
                r.Severity,
                r.Status,
                r.CreatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid referralId, string status, string clinicianId, string rationale, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Referrals.FindAsync([referralId], cancellationToken)
            ?? throw new InvalidOperationException($"Referral '{referralId}' not found.");

        entity.Status = status;
        entity.ClinicianId = clinicianId;
        entity.Rationale = rationale;
        entity.ResolvedAtUtc = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateAsync(Guid id, Guid tenantId, Guid patientId, string patientName, string reason, string severity, CancellationToken cancellationToken = default)
    {
        var entity = new ReferralEntity
        {
            Id = id,
            TenantId = tenantId,
            PatientId = patientId,
            PatientName = patientName,
            Reason = reason,
            Severity = severity,
            Status = "Pending",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        _db.Referrals.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
