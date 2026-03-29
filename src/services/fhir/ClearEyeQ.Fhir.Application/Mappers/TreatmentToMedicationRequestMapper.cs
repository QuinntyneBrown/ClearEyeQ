using ClearEyeQ.Fhir.Application.Interfaces;
using Hl7.Fhir.Model;

namespace ClearEyeQ.Fhir.Application.Mappers;

/// <summary>
/// Maps Treatment plan data to a FHIR MedicationRequest resource.
/// </summary>
public static class TreatmentToMedicationRequestMapper
{
    public static MedicationRequest Map(TreatmentData treatment, Guid patientId)
    {
        ArgumentNullException.ThrowIfNull(treatment);

        return new MedicationRequest
        {
            Id = treatment.TreatmentPlanId.ToString(),
            Status = treatment.Status.ToLowerInvariant() switch
            {
                "active" or "activated" => MedicationRequest.MedicationrequestStatus.Active,
                "proposed" => MedicationRequest.MedicationrequestStatus.Draft,
                "completed" => MedicationRequest.MedicationrequestStatus.Completed,
                "cancelled" or "rejected" => MedicationRequest.MedicationrequestStatus.Cancelled,
                _ => MedicationRequest.MedicationrequestStatus.Unknown
            },
            Intent = MedicationRequest.MedicationRequestIntent.Proposal,
            Subject = new ResourceReference($"Patient/{patientId}"),
            AuthoredOn = treatment.ProposedAtUtc.ToString("O"),
            Medication = new CodeableConcept("http://cleareyeq.com/fhir/treatment", "intervention", treatment.InterventionSummary),
            Note =
            {
                new Annotation { Text = new Markdown(treatment.InterventionSummary) }
            }
        };
    }
}
