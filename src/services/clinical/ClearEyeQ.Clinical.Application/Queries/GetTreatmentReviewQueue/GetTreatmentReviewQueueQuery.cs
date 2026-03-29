using MediatR;

namespace ClearEyeQ.Clinical.Application.Queries.GetTreatmentReviewQueue;

public sealed record GetTreatmentReviewQueueQuery(Guid TenantId) : IRequest<IReadOnlyList<TreatmentReviewDto>>;
