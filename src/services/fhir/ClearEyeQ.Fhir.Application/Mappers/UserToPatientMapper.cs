using ClearEyeQ.Fhir.Application.Interfaces;
using Hl7.Fhir.Model;

namespace ClearEyeQ.Fhir.Application.Mappers;

/// <summary>
/// Maps internal patient data to a FHIR Patient resource.
/// </summary>
public static class UserToPatientMapper
{
    public static Patient Map(PatientData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var patient = new Patient
        {
            Id = data.PatientId.ToString(),
            Name =
            {
                new HumanName
                {
                    Family = data.FamilyName,
                    Given = new[] { data.GivenName }
                }
            },
            Active = true
        };

        if (data.BirthDate.HasValue)
        {
            patient.BirthDate = data.BirthDate.Value.ToString("yyyy-MM-dd");
        }

        if (!string.IsNullOrEmpty(data.Gender))
        {
            patient.Gender = data.Gender.ToLowerInvariant() switch
            {
                "male" => AdministrativeGender.Male,
                "female" => AdministrativeGender.Female,
                "other" => AdministrativeGender.Other,
                _ => AdministrativeGender.Unknown
            };
        }

        return patient;
    }
}
