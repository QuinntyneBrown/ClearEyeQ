using ClearEyeQ.Fhir.Application.Interfaces;
using ClearEyeQ.Fhir.Application.Mappers;
using Hl7.Fhir.Model;
using MediatR;

namespace ClearEyeQ.Fhir.Application.Queries.GetFhirObservations;

public sealed class GetFhirObservationsHandler : IRequestHandler<GetFhirObservationsQuery, Bundle>
{
    private readonly IContextDataGatherer _dataGatherer;

    public GetFhirObservationsHandler(IContextDataGatherer dataGatherer)
    {
        _dataGatherer = dataGatherer;
    }

    public async Task<Bundle> Handle(GetFhirObservationsQuery request, CancellationToken cancellationToken)
    {
        var data = await _dataGatherer.GatherAsync(request.TenantId, request.PatientId, cancellationToken);

        var bundle = new Bundle
        {
            Type = Bundle.BundleType.Searchset,
            Total = data.Scans.Count,
            Timestamp = DateTimeOffset.UtcNow
        };

        foreach (var scan in data.Scans)
        {
            var observation = ScanToObservationMapper.Map(scan, data.PatientId);
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                Resource = observation,
                FullUrl = $"Observation/{observation.Id}"
            });
        }

        return bundle;
    }
}
