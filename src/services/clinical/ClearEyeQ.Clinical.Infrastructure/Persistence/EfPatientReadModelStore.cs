using ClearEyeQ.Clinical.Application.Interfaces;
using ClearEyeQ.Clinical.Application.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace ClearEyeQ.Clinical.Infrastructure.Persistence;

public sealed class EfPatientReadModelStore : IPatientReadModelStore
{
    private readonly ClinicalDbContext _db;

    public EfPatientReadModelStore(ClinicalDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PatientSummaryReadModel>> GetPatientListAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _db.PatientSummaries
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.UpdatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<PatientSummaryReadModel?> GetPatientSummaryAsync(Guid tenantId, Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _db.PatientSummaries
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.PatientId == patientId, cancellationToken);
    }

    public async Task<IReadOnlyList<ScanResultReadModel>> GetScanResultsAsync(Guid tenantId, Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _db.ScanResults
            .Where(s => s.TenantId == tenantId && s.PatientId == patientId)
            .OrderByDescending(s => s.CompletedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DiagnosticReadModel>> GetDiagnosticsAsync(Guid tenantId, Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _db.Diagnostics
            .Where(d => d.TenantId == tenantId && d.PatientId == patientId)
            .OrderByDescending(d => d.CompletedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TreatmentPlanReadModel>> GetTreatmentPlansAsync(Guid tenantId, Guid patientId, CancellationToken cancellationToken = default)
    {
        var query = _db.TreatmentPlans.Where(t => t.TenantId == tenantId);

        if (patientId != Guid.Empty)
        {
            query = query.Where(t => t.PatientId == patientId);
        }

        return await query
            .OrderByDescending(t => t.ProposedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertPatientSummaryAsync(PatientSummaryReadModel model, CancellationToken cancellationToken = default)
    {
        var existing = await _db.PatientSummaries
            .FirstOrDefaultAsync(p => p.TenantId == model.TenantId && p.PatientId == model.PatientId, cancellationToken);

        if (existing is null)
        {
            model.Id = Guid.NewGuid();
            _db.PatientSummaries.Add(model);
        }
        else
        {
            existing.Name = model.Name;
            existing.LastScanDate = model.LastScanDate ?? existing.LastScanDate;
            existing.RednessScore = model.RednessScore ?? existing.RednessScore;
            existing.Status = model.Status;
            existing.LatestDiagnosis = model.LatestDiagnosis ?? existing.LatestDiagnosis;
            existing.ActiveTreatment = model.ActiveTreatment ?? existing.ActiveTreatment;
            existing.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertScanResultAsync(ScanResultReadModel model, CancellationToken cancellationToken = default)
    {
        var existing = await _db.ScanResults
            .FirstOrDefaultAsync(s => s.TenantId == model.TenantId && s.ScanId == model.ScanId, cancellationToken);

        if (existing is null)
        {
            model.Id = Guid.NewGuid();
            _db.ScanResults.Add(model);
        }
        else
        {
            existing.RednessScore = model.RednessScore;
            existing.TearFilmStability = model.TearFilmStability;
            existing.ConfidenceScore = model.ConfidenceScore;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertDiagnosticAsync(DiagnosticReadModel model, CancellationToken cancellationToken = default)
    {
        var existing = await _db.Diagnostics
            .FirstOrDefaultAsync(d => d.TenantId == model.TenantId && d.DiagnosticSessionId == model.DiagnosticSessionId, cancellationToken);

        if (existing is null)
        {
            model.Id = Guid.NewGuid();
            _db.Diagnostics.Add(model);
        }
        else
        {
            existing.PrimaryDiagnosis = model.PrimaryDiagnosis;
            existing.Severity = model.Severity;
            existing.ConfidenceScore = model.ConfidenceScore;
            existing.CausalFactors = model.CausalFactors;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertTreatmentPlanAsync(TreatmentPlanReadModel model, CancellationToken cancellationToken = default)
    {
        var existing = await _db.TreatmentPlans
            .FirstOrDefaultAsync(t => t.TenantId == model.TenantId && t.TreatmentPlanId == model.TreatmentPlanId, cancellationToken);

        if (existing is null)
        {
            model.Id = Guid.NewGuid();
            _db.TreatmentPlans.Add(model);
        }
        else
        {
            existing.Status = model.Status;
            existing.InterventionSummary = model.InterventionSummary;
            existing.Rationale = model.Rationale;
            existing.ActivatedAtUtc = model.ActivatedAtUtc ?? existing.ActivatedAtUtc;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
