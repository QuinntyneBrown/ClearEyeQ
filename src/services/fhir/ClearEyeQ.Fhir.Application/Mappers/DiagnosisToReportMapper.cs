using ClearEyeQ.Fhir.Application.Interfaces;
using Hl7.Fhir.Model;

namespace ClearEyeQ.Fhir.Application.Mappers;

/// <summary>
/// Maps DiagnosticSession data to a FHIR DiagnosticReport resource.
/// </summary>
public static class DiagnosisToReportMapper
{
    public static DiagnosticReport Map(DiagnosticData diagnostic, Guid patientId)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);

        return new DiagnosticReport
        {
            Id = diagnostic.DiagnosticSessionId.ToString(),
            Status = DiagnosticReport.DiagnosticReportStatus.Final,
            Code = new CodeableConcept("http://cleareyeq.com/fhir", "ocular-diagnostic", "Ocular Diagnostic Report"),
            Subject = new ResourceReference($"Patient/{patientId}"),
            Effective = new FhirDateTime(diagnostic.CompletedAtUtc.ToString("O")),
            Conclusion = diagnostic.PrimaryDiagnosis,
            ConclusionCode =
            {
                new CodeableConcept("http://cleareyeq.com/fhir/diagnosis", diagnostic.PrimaryDiagnosis, diagnostic.PrimaryDiagnosis)
            }
        };
    }
}
