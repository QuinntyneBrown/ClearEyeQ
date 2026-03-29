using ClearEyeQ.Fhir.Application.Interfaces;
using ClearEyeQ.Fhir.Application.Mappers;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using MediatR;

namespace ClearEyeQ.Fhir.Application.Commands.ExportPatientData;

public sealed class ExportPatientDataHandler : IRequestHandler<ExportPatientDataCommand, ExportPatientDataResult>
{
    private readonly IContextDataGatherer _dataGatherer;
    private readonly IFhirBundleStore _bundleStore;

    public ExportPatientDataHandler(
        IContextDataGatherer dataGatherer,
        IFhirBundleStore bundleStore)
    {
        _dataGatherer = dataGatherer;
        _bundleStore = bundleStore;
    }

    public async Task<ExportPatientDataResult> Handle(ExportPatientDataCommand request, CancellationToken cancellationToken)
    {
        var data = await _dataGatherer.GatherAsync(request.TenantId, request.PatientId, cancellationToken);

        var bundle = new Bundle
        {
            Type = Bundle.BundleType.Collection,
            Timestamp = DateTimeOffset.UtcNow
        };

        // Map patient
        var patient = UserToPatientMapper.Map(data);
        bundle.Entry.Add(new Bundle.EntryComponent { Resource = patient });

        // Map scans to observations
        foreach (var scan in data.Scans)
        {
            var observation = ScanToObservationMapper.Map(scan, data.PatientId);
            bundle.Entry.Add(new Bundle.EntryComponent { Resource = observation });
        }

        // Map diagnostics to reports
        foreach (var diagnostic in data.Diagnostics)
        {
            var report = DiagnosisToReportMapper.Map(diagnostic, data.PatientId);
            bundle.Entry.Add(new Bundle.EntryComponent { Resource = report });
        }

        // Map treatments to medication requests
        foreach (var treatment in data.Treatments)
        {
            var medRequest = TreatmentToMedicationRequestMapper.Map(treatment, data.PatientId);
            bundle.Entry.Add(new Bundle.EntryComponent { Resource = medRequest });
        }

        var serializer = new FhirJsonSerializer(new SerializerSettings { Pretty = false });
        var bundleJson = serializer.SerializeToString(bundle);

        var blobName = await _bundleStore.StoreAsync(request.TenantId, request.PatientId, bundleJson, cancellationToken);

        return new ExportPatientDataResult(blobName, bundle.Entry.Count);
    }
}
