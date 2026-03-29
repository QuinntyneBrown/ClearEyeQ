using Hl7.Fhir.Model;
using MediatR;

namespace ClearEyeQ.Fhir.Application.Queries.GetFhirPatient;

public sealed record GetFhirPatientQuery(Guid TenantId, Guid PatientId) : IRequest<Patient?>;
