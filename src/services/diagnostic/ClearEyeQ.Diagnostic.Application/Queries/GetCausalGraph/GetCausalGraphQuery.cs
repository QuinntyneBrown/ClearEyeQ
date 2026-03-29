using MediatR;

namespace ClearEyeQ.Diagnostic.Application.Queries.GetCausalGraph;

public sealed record GetCausalGraphQuery(Guid SessionId, Guid TenantId) : IRequest<CausalGraphDetailDto?>;
