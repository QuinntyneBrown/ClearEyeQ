using ClearEyeQ.Clinical.Application.Interfaces;
using MediatR;

namespace ClearEyeQ.Clinical.Application.Queries.GetPatientList;

public sealed class GetPatientListHandler : IRequestHandler<GetPatientListQuery, IReadOnlyList<PatientSummaryDto>>
{
    private readonly IPatientReadModelStore _store;

    public GetPatientListHandler(IPatientReadModelStore store)
    {
        _store = store;
    }

    public async Task<IReadOnlyList<PatientSummaryDto>> Handle(GetPatientListQuery request, CancellationToken cancellationToken)
    {
        var patients = await _store.GetPatientListAsync(request.TenantId, cancellationToken);

        return patients.Select(p => new PatientSummaryDto(
            p.PatientId,
            p.Name,
            p.LastScanDate,
            p.RednessScore,
            p.Status)).ToList();
    }
}
