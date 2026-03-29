using MediatR;

namespace ClearEyeQ.Billing.Application.Commands.RecordUsage;

public sealed record RecordUsageCommand(Guid TenantId) : IRequest;
