using ClearEyeQ.Billing.Application.Interfaces;
using ClearEyeQ.Billing.Domain.Entities;
using ClearEyeQ.Billing.Domain.Enums;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Billing.Application.Queries.CheckFeatureAccess;

public sealed class CheckFeatureAccessHandler : IRequestHandler<CheckFeatureAccessQuery, FeatureAccessDto>
{
    private readonly ISubscriptionRepository _repository;

    public CheckFeatureAccessHandler(ISubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<FeatureAccessDto> Handle(CheckFeatureAccessQuery request, CancellationToken cancellationToken)
    {
        var tenantId = new TenantId(request.TenantId);
        var subscription = await _repository.GetByTenantAsync(tenantId, cancellationToken);

        if (subscription is null)
            return new FeatureAccessDto(false, "No active subscription found.");

        if (subscription.Status is SubscriptionStatus.Cancelled or SubscriptionStatus.Suspended)
            return new FeatureAccessDto(false, $"Subscription is {subscription.Status}.");

        var plan = Plan.Create(subscription.PlanTier);
        var featureSet = plan.FeatureSet;

        var hasAccess = request.FeatureName.ToLowerInvariant() switch
        {
            "predictive" => featureSet.PredictiveAccess,
            "autonomous_treatment" => featureSet.AutonomousTreatment,
            "priority_support" => featureSet.PrioritySupport,
            "scan" => !subscription.UsageMeter.HasReachedLimit(),
            _ => true
        };

        var reason = hasAccess
            ? null
            : request.FeatureName.ToLowerInvariant() == "scan"
                ? $"Scan limit reached ({subscription.UsageMeter.ScanCount}/{subscription.UsageMeter.ScanLimit})."
                : $"Feature '{request.FeatureName}' requires a higher plan tier.";

        return new FeatureAccessDto(hasAccess, reason);
    }
}
