using MediatR;

namespace ClearEyeQ.Treatment.Application.Commands.RecordEfficacy;

public sealed record RecordEfficacyCommand(
    Guid PlanId,
    Guid TenantId,
    double RednessScore,
    double BaselineScore) : IRequest;
