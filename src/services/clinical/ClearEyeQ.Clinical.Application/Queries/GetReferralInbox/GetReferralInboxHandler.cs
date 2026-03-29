using ClearEyeQ.Clinical.Application.Interfaces;
using MediatR;

namespace ClearEyeQ.Clinical.Application.Queries.GetReferralInbox;

public sealed class GetReferralInboxHandler : IRequestHandler<GetReferralInboxQuery, IReadOnlyList<ReferralDto>>
{
    private readonly IReferralRepository _referralRepository;

    public GetReferralInboxHandler(IReferralRepository referralRepository)
    {
        _referralRepository = referralRepository;
    }

    public async Task<IReadOnlyList<ReferralDto>> Handle(GetReferralInboxQuery request, CancellationToken cancellationToken)
    {
        return await _referralRepository.GetPendingReferralsAsync(request.TenantId, cancellationToken);
    }
}
