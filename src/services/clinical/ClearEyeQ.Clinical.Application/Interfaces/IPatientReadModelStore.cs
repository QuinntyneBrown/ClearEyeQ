using ClearEyeQ.Clinical.Application.ReadModels;

namespace ClearEyeQ.Clinical.Application.Interfaces;

/// <summary>
/// Read model store for patient-related projections used by the clinical portal.
/// </summary>
public interface IPatientReadModelStore
{
    Task<IReadOnlyList<PatientSummaryReadModel>> GetPatientListAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<PatientSummaryReadModel?> GetPatientSummaryAsync(Guid tenantId, Guid patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScanResultReadModel>> GetScanResultsAsync(Guid tenantId, Guid patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DiagnosticReadModel>> GetDiagnosticsAsync(Guid tenantId, Guid patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TreatmentPlanReadModel>> GetTreatmentPlansAsync(Guid tenantId, Guid patientId, CancellationToken cancellationToken = default);
    Task UpsertPatientSummaryAsync(PatientSummaryReadModel model, CancellationToken cancellationToken = default);
    Task UpsertScanResultAsync(ScanResultReadModel model, CancellationToken cancellationToken = default);
    Task UpsertDiagnosticAsync(DiagnosticReadModel model, CancellationToken cancellationToken = default);
    Task UpsertTreatmentPlanAsync(TreatmentPlanReadModel model, CancellationToken cancellationToken = default);
}
