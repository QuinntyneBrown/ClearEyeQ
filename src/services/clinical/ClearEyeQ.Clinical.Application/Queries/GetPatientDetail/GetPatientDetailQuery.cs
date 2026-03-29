using MediatR;

namespace ClearEyeQ.Clinical.Application.Queries.GetPatientDetail;

public sealed record GetPatientDetailQuery(Guid TenantId, Guid PatientId) : IRequest<PatientDetailDto?>;
