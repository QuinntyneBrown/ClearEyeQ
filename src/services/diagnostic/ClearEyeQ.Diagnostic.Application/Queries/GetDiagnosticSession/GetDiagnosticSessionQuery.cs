using MediatR;

namespace ClearEyeQ.Diagnostic.Application.Queries.GetDiagnosticSession;

public sealed record GetDiagnosticSessionQuery(Guid SessionId, Guid TenantId) : IRequest<DiagnosticSessionDto?>;
