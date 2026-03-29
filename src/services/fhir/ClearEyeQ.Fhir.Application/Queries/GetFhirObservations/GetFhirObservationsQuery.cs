using Hl7.Fhir.Model;
using MediatR;

namespace ClearEyeQ.Fhir.Application.Queries.GetFhirObservations;

public sealed record GetFhirObservationsQuery(Guid TenantId, Guid PatientId) : IRequest<Bundle>;
