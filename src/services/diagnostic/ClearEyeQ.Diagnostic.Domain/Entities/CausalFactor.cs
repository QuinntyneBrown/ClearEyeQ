using ClearEyeQ.Diagnostic.Domain.Enums;

namespace ClearEyeQ.Diagnostic.Domain.Entities;

public sealed class CausalFactor
{
    public string FactorId { get; private set; }
    public string Label { get; private set; }
    public CausalCategory CausalCategory { get; private set; }
    public double Weight { get; private set; }

    private CausalFactor() { FactorId = string.Empty; Label = string.Empty; }

    public CausalFactor(string factorId, string label, CausalCategory causalCategory, double weight)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(factorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        FactorId = factorId;
        Label = label;
        CausalCategory = causalCategory;
        Weight = weight;
    }
}
