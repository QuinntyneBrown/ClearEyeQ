using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using ClearEyeQ.Treatment.Application.Interfaces;
using MediatR;

namespace ClearEyeQ.Treatment.Application.Commands.RejectTreatmentPlan;

public sealed class RejectTreatmentPlanHandler : IRequestHandler<RejectTreatmentPlanCommand>
{
    private readonly ITreatmentPlanRepository _repository;

    public RejectTreatmentPlanHandler(ITreatmentPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(RejectTreatmentPlanCommand request, CancellationToken cancellationToken)
    {
        var tenantId = new TenantId(request.TenantId);
        var plan = await _repository.GetByIdAsync(request.PlanId, tenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Treatment plan {request.PlanId} not found.");

        plan.Reject(request.Reason);

        await _repository.UpdateAsync(plan, cancellationToken);
    }
}
