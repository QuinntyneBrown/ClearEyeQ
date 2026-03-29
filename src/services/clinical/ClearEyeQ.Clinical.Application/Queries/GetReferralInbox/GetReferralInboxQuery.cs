using MediatR;

namespace ClearEyeQ.Clinical.Application.Queries.GetReferralInbox;

public sealed record GetReferralInboxQuery(Guid TenantId) : IRequest<IReadOnlyList<ReferralDto>>;
