using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using ClearEyeQ.Treatment.Application.Interfaces;
using MediatR;

namespace ClearEyeQ.Treatment.Application.Commands.RecordEfficacy;

public sealed class RecordEfficacyHandler : IRequestHandler<RecordEfficacyCommand>
{
    private readonly ITreatmentPlanRepository _repository;
    private readonly IEfficacyCalculator _calculator;

    public RecordEfficacyHandler(
        ITreatmentPlanRepository repository,
        IEfficacyCalculator calculator)
    {
        _repository = repository;
        _calculator = calculator;
    }

    public async Task Handle(RecordEfficacyCommand request, CancellationToken cancellationToken)
    {
        var tenantId = new TenantId(request.TenantId);
        var plan = await _repository.GetByIdAsync(request.PlanId, tenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Treatment plan {request.PlanId} not found.");

        plan.RecordEfficacy(request.RednessScore, request.BaselineScore);

        if (_calculator.IsResolved(plan))
        {
            plan.VerifyResolution();
        }

        await _repository.UpdateAsync(plan, cancellationToken);
    }
}
