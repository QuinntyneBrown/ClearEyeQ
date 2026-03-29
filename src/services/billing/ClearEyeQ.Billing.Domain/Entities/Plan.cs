using ClearEyeQ.Billing.Domain.Enums;
using ClearEyeQ.Billing.Domain.ValueObjects;

namespace ClearEyeQ.Billing.Domain.Entities;

public sealed class Plan
{
    public Guid PlanId { get; private set; } = Guid.NewGuid();
    public PlanTier Tier { get; private set; }
    public string Name { get; private set; } = default!;
    public decimal MonthlyPrice { get; private set; }
    public FeatureSet FeatureSet { get; private set; } = default!;

    private Plan() { }

    public static Plan Create(PlanTier tier) => tier switch
    {
        PlanTier.Free => new Plan
        {
            Tier = PlanTier.Free,
            Name = "Free",
            MonthlyPrice = 0.00m,
            FeatureSet = FeatureSet.ForFree()
        },
        PlanTier.Pro => new Plan
        {
            Tier = PlanTier.Pro,
            Name = "Pro",
            MonthlyPrice = 9.99m,
            FeatureSet = FeatureSet.ForPro()
        },
        PlanTier.Premium => new Plan
        {
            Tier = PlanTier.Premium,
            Name = "Premium",
            MonthlyPrice = 29.99m,
            FeatureSet = FeatureSet.ForPremium()
        },
        PlanTier.Autonomous => new Plan
        {
            Tier = PlanTier.Autonomous,
            Name = "Autonomous",
            MonthlyPrice = 99.99m,
            FeatureSet = FeatureSet.ForAutonomous()
        },
        _ => throw new ArgumentOutOfRangeException(nameof(tier))
    };
}
