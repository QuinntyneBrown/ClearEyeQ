using MediatR;

namespace ClearEyeQ.Clinical.Application.Queries.GetPatientList;

public sealed record GetPatientListQuery(Guid TenantId) : IRequest<IReadOnlyList<PatientSummaryDto>>;
