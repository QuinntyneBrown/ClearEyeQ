using MediatR;

namespace ClearEyeQ.Billing.Application.Queries.CheckFeatureAccess;

public sealed record CheckFeatureAccessQuery(
    Guid TenantId,
    string FeatureName) : IRequest<FeatureAccessDto>;
