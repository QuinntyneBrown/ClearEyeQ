using ClearEyeQ.Monitoring.Domain.Enums;
using MediatR;

namespace ClearEyeQ.Monitoring.Application.Commands.IngestWearableData;

public sealed record IngestWearableDataCommand(
    Guid UserId,
    Guid TenantId,
    WearableSource Source,
    MetricType MetricType,
    double Value,
    DateTimeOffset Timestamp) : IRequest<Guid>;
