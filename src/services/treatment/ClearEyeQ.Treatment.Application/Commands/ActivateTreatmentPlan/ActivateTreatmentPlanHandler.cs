using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using ClearEyeQ.Treatment.Application.Interfaces;
using MediatR;

namespace ClearEyeQ.Treatment.Application.Commands.ActivateTreatmentPlan;

public sealed class ActivateTreatmentPlanHandler : IRequestHandler<ActivateTreatmentPlanCommand>
{
    private readonly ITreatmentPlanRepository _repository;

    public ActivateTreatmentPlanHandler(ITreatmentPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(ActivateTreatmentPlanCommand request, CancellationToken cancellationToken)
    {
        var tenantId = new TenantId(request.TenantId);
        var plan = await _repository.GetByIdAsync(request.PlanId, tenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Treatment plan {request.PlanId} not found.");

        plan.Activate();

        await _repository.UpdateAsync(plan, cancellationToken);
    }
}
