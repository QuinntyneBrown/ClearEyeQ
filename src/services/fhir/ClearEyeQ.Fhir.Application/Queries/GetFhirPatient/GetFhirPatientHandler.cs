using ClearEyeQ.Fhir.Application.Interfaces;
using ClearEyeQ.Fhir.Application.Mappers;
using Hl7.Fhir.Model;
using MediatR;

namespace ClearEyeQ.Fhir.Application.Queries.GetFhirPatient;

public sealed class GetFhirPatientHandler : IRequestHandler<GetFhirPatientQuery, Patient?>
{
    private readonly IContextDataGatherer _dataGatherer;

    public GetFhirPatientHandler(IContextDataGatherer dataGatherer)
    {
        _dataGatherer = dataGatherer;
    }

    public async Task<Patient?> Handle(GetFhirPatientQuery request, CancellationToken cancellationToken)
    {
        var data = await _dataGatherer.GatherAsync(request.TenantId, request.PatientId, cancellationToken);

        if (string.IsNullOrEmpty(data.GivenName) && string.IsNullOrEmpty(data.FamilyName))
        {
            return null;
        }

        return UserToPatientMapper.Map(data);
    }
}
