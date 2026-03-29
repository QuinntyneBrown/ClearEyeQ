using MediatR;

namespace ClearEyeQ.Diagnostic.Application.Commands.GenerateDiagnosis;

public sealed record GenerateDiagnosisCommand(
    Guid ScanId,
    Guid UserId,
    Guid TenantId) : IRequest<Guid>;
