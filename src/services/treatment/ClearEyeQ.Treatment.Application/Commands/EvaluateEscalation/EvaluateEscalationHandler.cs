using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using ClearEyeQ.Treatment.Application.Interfaces;
using MediatR;

namespace ClearEyeQ.Treatment.Application.Commands.EvaluateEscalation;

public sealed class EvaluateEscalationHandler : IRequestHandler<EvaluateEscalationCommand, bool>
{
    private readonly ITreatmentPlanRepository _repository;

    public EvaluateEscalationHandler(ITreatmentPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(EvaluateEscalationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = new TenantId(request.TenantId);
        var plan = await _repository.GetByIdAsync(request.PlanId, tenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Treatment plan {request.PlanId} not found.");

        var shouldEscalate = plan.EvaluateEfficacy();

        if (shouldEscalate)
        {
            plan.Escalate();
            await _repository.UpdateAsync(plan, cancellationToken);
        }

        return shouldEscalate;
    }
}
