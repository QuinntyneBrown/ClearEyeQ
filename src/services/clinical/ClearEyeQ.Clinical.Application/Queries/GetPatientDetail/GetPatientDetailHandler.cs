using ClearEyeQ.Clinical.Application.Interfaces;
using MediatR;

namespace ClearEyeQ.Clinical.Application.Queries.GetPatientDetail;

public sealed class GetPatientDetailHandler : IRequestHandler<GetPatientDetailQuery, PatientDetailDto?>
{
    private readonly IPatientReadModelStore _store;

    public GetPatientDetailHandler(IPatientReadModelStore store)
    {
        _store = store;
    }

    public async Task<PatientDetailDto?> Handle(GetPatientDetailQuery request, CancellationToken cancellationToken)
    {
        var summary = await _store.GetPatientSummaryAsync(request.TenantId, request.PatientId, cancellationToken);
        if (summary is null)
        {
            return null;
        }

        var scans = await _store.GetScanResultsAsync(request.TenantId, request.PatientId, cancellationToken);
        var diagnostics = await _store.GetDiagnosticsAsync(request.TenantId, request.PatientId, cancellationToken);
        var treatments = await _store.GetTreatmentPlansAsync(request.TenantId, request.PatientId, cancellationToken);

        return new PatientDetailDto(
            summary.PatientId,
            summary.Name,
            summary.Status,
            scans.Select(s => new ScanDto(
                s.ScanId,
                s.EyeSide,
                s.RednessScore,
                s.TearFilmStability,
                s.ConfidenceScore,
                s.CompletedAtUtc)).ToList(),
            diagnostics.Select(d => new DiagnosisDto(
                d.DiagnosticSessionId,
                d.PrimaryDiagnosis,
                d.Severity,
                d.ConfidenceScore,
                d.CompletedAtUtc)).ToList(),
            treatments.Select(t => new TreatmentDto(
                t.TreatmentPlanId,
                t.Status,
                t.InterventionSummary,
                t.ProposedAtUtc,
                t.ActivatedAtUtc)).ToList());
    }
}
