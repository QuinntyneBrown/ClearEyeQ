using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using ClearEyeQ.Treatment.Application.Interfaces;
using ClearEyeQ.Treatment.Domain.Aggregates;
using ClearEyeQ.Treatment.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Treatment.Application.Commands.CreateTreatmentPlan;

public sealed class CreateTreatmentPlanHandler : IRequestHandler<CreateTreatmentPlanCommand, Guid>
{
    private readonly ITreatmentPlanRepository _repository;

    public CreateTreatmentPlanHandler(ITreatmentPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateTreatmentPlanCommand request, CancellationToken cancellationToken)
    {
        EscalationRule? escalationRule = null;
        if (request.EscalationDaysThreshold.HasValue &&
            request.EscalationMinImprovementPercent.HasValue &&
            request.EscalationAction is not null)
        {
            escalationRule = new EscalationRule(
                request.EscalationDaysThreshold.Value,
                request.EscalationMinImprovementPercent.Value,
                request.EscalationAction);
        }

        var plan = TreatmentPlan.Propose(
            new UserId(request.UserId),
            new TenantId(request.TenantId),
            request.DiagnosisId,
            escalationRule);

        await _repository.AddAsync(plan, cancellationToken);

        return plan.PlanId;
    }
}
