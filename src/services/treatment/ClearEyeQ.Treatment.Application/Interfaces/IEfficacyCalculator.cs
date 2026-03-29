using ClearEyeQ.Treatment.Domain.Aggregates;

namespace ClearEyeQ.Treatment.Application.Interfaces;

public interface IEfficacyCalculator
{
    double CalculateImprovement(TreatmentPlan plan);
    bool ShouldEscalate(TreatmentPlan plan);
    bool IsResolved(TreatmentPlan plan);
}
