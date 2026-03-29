using MediatR;

namespace ClearEyeQ.Monitoring.Application.Queries.GetMonitoringDashboard;

public sealed record GetMonitoringDashboardQuery(
    Guid UserId,
    Guid TenantId) : IRequest<MonitoringDashboardDto>;
