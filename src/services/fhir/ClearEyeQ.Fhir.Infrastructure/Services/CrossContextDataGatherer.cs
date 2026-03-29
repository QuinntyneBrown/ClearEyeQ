using System.Net.Http.Json;
using ClearEyeQ.Fhir.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Fhir.Infrastructure.Services;

/// <summary>
/// Calls other service APIs to gather patient data from upstream bounded contexts.
/// </summary>
public sealed class CrossContextDataGatherer : IContextDataGatherer
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CrossContextDataGatherer> _logger;

    public CrossContextDataGatherer(
        IHttpClientFactory httpClientFactory,
        ILogger<CrossContextDataGatherer> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<PatientData> GatherAsync(Guid tenantId, Guid patientId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Gathering patient data for {PatientId} in tenant {TenantId}",
            patientId, tenantId);

        var scans = await GatherScansAsync(tenantId, patientId, cancellationToken);
        var diagnostics = await GatherDiagnosticsAsync(tenantId, patientId, cancellationToken);
        var treatments = await GatherTreatmentsAsync(tenantId, patientId, cancellationToken);
        var patientInfo = await GatherPatientInfoAsync(tenantId, patientId, cancellationToken);

        return new PatientData(
            PatientId: patientId,
            GivenName: patientInfo.GivenName,
            FamilyName: patientInfo.FamilyName,
            BirthDate: patientInfo.BirthDate,
            Gender: patientInfo.Gender,
            Scans: scans,
            Diagnostics: diagnostics,
            Treatments: treatments);
    }

    private async Task<IReadOnlyList<ScanData>> GatherScansAsync(Guid tenantId, Guid patientId, CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ClinicalApi");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
            var response = await client.GetAsync($"api/clinical/patients/{patientId}", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to gather scan data for patient {PatientId}: {Status}", patientId, response.StatusCode);
                return [];
            }

            var detail = await response.Content.ReadFromJsonAsync<PatientDetailResponse>(cancellationToken: ct);
            return detail?.Scans.Select(s => new ScanData(
                s.ScanId, s.EyeSide, s.RednessScore, s.TearFilmStability, s.ConfidenceScore, s.CompletedAtUtc)).ToList()
                ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error gathering scan data for patient {PatientId}", patientId);
            return [];
        }
    }

    private async Task<IReadOnlyList<DiagnosticData>> GatherDiagnosticsAsync(Guid tenantId, Guid patientId, CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ClinicalApi");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
            var response = await client.GetAsync($"api/clinical/patients/{patientId}", ct);

            if (!response.IsSuccessStatusCode)
            {
                return [];
            }

            var detail = await response.Content.ReadFromJsonAsync<PatientDetailResponse>(cancellationToken: ct);
            return detail?.Diagnostics.Select(d => new DiagnosticData(
                d.DiagnosticSessionId, d.PrimaryDiagnosis, d.Severity, d.ConfidenceScore, string.Empty, d.CompletedAtUtc)).ToList()
                ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error gathering diagnostic data for patient {PatientId}", patientId);
            return [];
        }
    }

    private async Task<IReadOnlyList<TreatmentData>> GatherTreatmentsAsync(Guid tenantId, Guid patientId, CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ClinicalApi");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
            var response = await client.GetAsync($"api/clinical/patients/{patientId}", ct);

            if (!response.IsSuccessStatusCode)
            {
                return [];
            }

            var detail = await response.Content.ReadFromJsonAsync<PatientDetailResponse>(cancellationToken: ct);
            return detail?.Treatments.Select(t => new TreatmentData(
                t.TreatmentPlanId, t.Status, t.InterventionSummary, t.ProposedAtUtc)).ToList()
                ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error gathering treatment data for patient {PatientId}", patientId);
            return [];
        }
    }

    private async Task<PatientInfoResponse> GatherPatientInfoAsync(Guid tenantId, Guid patientId, CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("IdentityApi");
            client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
            var response = await client.GetAsync($"api/identity/users/{patientId}", ct);

            if (!response.IsSuccessStatusCode)
            {
                return new PatientInfoResponse("Unknown", "Patient", null, null);
            }

            return await response.Content.ReadFromJsonAsync<PatientInfoResponse>(cancellationToken: ct)
                ?? new PatientInfoResponse("Unknown", "Patient", null, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error gathering patient info for {PatientId}", patientId);
            return new PatientInfoResponse("Unknown", "Patient", null, null);
        }
    }

    private sealed record PatientDetailResponse(
        List<ScanDetailResponse> Scans,
        List<DiagnosticDetailResponse> Diagnostics,
        List<TreatmentDetailResponse> Treatments);

    private sealed record ScanDetailResponse(
        Guid ScanId, string EyeSide, double RednessScore,
        double TearFilmStability, double ConfidenceScore, DateTimeOffset CompletedAtUtc);

    private sealed record DiagnosticDetailResponse(
        Guid DiagnosticSessionId, string PrimaryDiagnosis, string Severity,
        double ConfidenceScore, DateTimeOffset CompletedAtUtc);

    private sealed record TreatmentDetailResponse(
        Guid TreatmentPlanId, string Status, string InterventionSummary,
        DateTimeOffset ProposedAtUtc);

    private sealed record PatientInfoResponse(
        string GivenName, string FamilyName, DateTimeOffset? BirthDate, string? Gender);
}
