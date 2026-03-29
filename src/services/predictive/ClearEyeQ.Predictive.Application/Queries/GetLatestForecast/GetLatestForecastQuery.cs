using MediatR;

namespace ClearEyeQ.Predictive.Application.Queries.GetLatestForecast;

public sealed record GetLatestForecastQuery(Guid UserId, Guid TenantId) : IRequest<ForecastDto?>;
