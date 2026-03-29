using ClearEyeQ.Billing.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Billing.Application.Commands.RecordUsage;

public sealed class RecordUsageHandler : IRequestHandler<RecordUsageCommand>
{
    private readonly ISubscriptionRepository _repository;

    public RecordUsageHandler(ISubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(RecordUsageCommand request, CancellationToken cancellationToken)
    {
        var tenantId = new TenantId(request.TenantId);
        var subscription = await _repository.GetByTenantAsync(tenantId, cancellationToken)
            ?? throw new InvalidOperationException($"No subscription found for tenant {request.TenantId}.");

        subscription.RecordUsage();

        await _repository.UpdateAsync(subscription, cancellationToken);
    }
}
