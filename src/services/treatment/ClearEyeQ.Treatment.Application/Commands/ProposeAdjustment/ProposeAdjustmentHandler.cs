using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using ClearEyeQ.Treatment.Application.Interfaces;
using MediatR;

namespace ClearEyeQ.Treatment.Application.Commands.ProposeAdjustment;

public sealed class ProposeAdjustmentHandler : IRequestHandler<ProposeAdjustmentCommand>
{
    private readonly ITreatmentPlanRepository _repository;

    public ProposeAdjustmentHandler(ITreatmentPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(ProposeAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = new TenantId(request.TenantId);
        var plan = await _repository.GetByIdAsync(request.PlanId, tenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Treatment plan {request.PlanId} not found.");

        plan.ProposeAdjustment(request.Reason);

        await _repository.UpdateAsync(plan, cancellationToken);
    }
}
