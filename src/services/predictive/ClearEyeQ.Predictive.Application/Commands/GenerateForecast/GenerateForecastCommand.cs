using MediatR;

namespace ClearEyeQ.Predictive.Application.Commands.GenerateForecast;

public sealed record GenerateForecastCommand(
    Guid UserId,
    Guid TenantId,
    int ForecastDays = 3) : IRequest<Guid>;
