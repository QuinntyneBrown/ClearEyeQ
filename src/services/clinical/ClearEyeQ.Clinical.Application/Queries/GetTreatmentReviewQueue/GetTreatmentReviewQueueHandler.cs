using ClearEyeQ.Clinical.Application.Interfaces;
using MediatR;

namespace ClearEyeQ.Clinical.Application.Queries.GetTreatmentReviewQueue;

public sealed class GetTreatmentReviewQueueHandler : IRequestHandler<GetTreatmentReviewQueueQuery, IReadOnlyList<TreatmentReviewDto>>
{
    private readonly IPatientReadModelStore _store;

    public GetTreatmentReviewQueueHandler(IPatientReadModelStore store)
    {
        _store = store;
    }

    public async Task<IReadOnlyList<TreatmentReviewDto>> Handle(GetTreatmentReviewQueueQuery request, CancellationToken cancellationToken)
    {
        var plans = await _store.GetTreatmentPlansAsync(request.TenantId, Guid.Empty, cancellationToken);

        return plans
            .Where(p => p.Status is "Proposed" or "AdjustmentProposed")
            .Select(p => new TreatmentReviewDto(
                p.Id,
                p.PatientId,
                string.Empty,
                p.Status == "Proposed" ? "TreatmentPlan" : "TreatmentAdjustment",
                p.InterventionSummary,
                p.Rationale,
                p.Status,
                p.ProposedAtUtc))
            .ToList();
    }
}
