using MediatR;

namespace ClearEyeQ.Predictive.Application.Commands.DetectFlareUpRisk;

public sealed record DetectFlareUpRiskCommand(
    Guid UserId,
    Guid TenantId,
    List<string> ActiveConditions) : IRequest<FlareUpResultDto>;

public sealed record FlareUpResultDto(
    double Probability,
    string RiskLevel,
    List<string> TriggerFactors,
    List<string> PreventiveActions);
