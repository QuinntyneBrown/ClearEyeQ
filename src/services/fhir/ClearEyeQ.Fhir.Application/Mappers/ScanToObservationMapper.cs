using ClearEyeQ.Fhir.Application.Interfaces;
using Hl7.Fhir.Model;

namespace ClearEyeQ.Fhir.Application.Mappers;

/// <summary>
/// Maps Scan domain model data to a FHIR Observation resource.
/// </summary>
public static class ScanToObservationMapper
{
    public static Observation Map(ScanData scan, Guid patientId)
    {
        ArgumentNullException.ThrowIfNull(scan);

        return new Observation
        {
            Id = scan.ScanId.ToString(),
            Status = ObservationStatus.Final,
            Code = new CodeableConcept("http://loinc.org", "79880-1", "Eye redness assessment"),
            Subject = new ResourceReference($"Patient/{patientId}"),
            Effective = new FhirDateTime(scan.CompletedAtUtc.ToString("O")),
            Component =
            {
                new Observation.ComponentComponent
                {
                    Code = new CodeableConcept("http://cleareyeq.com/fhir", "redness-score", "Redness Score"),
                    Value = new Quantity(Convert.ToDecimal(scan.RednessScore), "score", "http://unitsofmeasure.org")
                },
                new Observation.ComponentComponent
                {
                    Code = new CodeableConcept("http://cleareyeq.com/fhir", "tear-film-stability", "Tear Film Stability"),
                    Value = new Quantity(Convert.ToDecimal(scan.TearFilmStability), "score", "http://unitsofmeasure.org")
                },
                new Observation.ComponentComponent
                {
                    Code = new CodeableConcept("http://cleareyeq.com/fhir", "confidence-score", "Confidence Score"),
                    Value = new Quantity(Convert.ToDecimal(scan.ConfidenceScore), "score", "http://unitsofmeasure.org")
                }
            },
            BodySite = new CodeableConcept("http://snomed.info/sct",
                scan.EyeSide.Equals("Left", StringComparison.OrdinalIgnoreCase) ? "8966001" : "18944008",
                $"{scan.EyeSide} eye")
        };
    }
}
