using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using MediatR;

namespace ClearEyeQ.Fhir.Application.Commands.ImportClinicalData;

public sealed class ImportClinicalDataHandler : IRequestHandler<ImportClinicalDataCommand, ImportClinicalDataResult>
{
    public async Task<ImportClinicalDataResult> Handle(ImportClinicalDataCommand request, CancellationToken cancellationToken)
    {
        var parser = new FhirJsonParser(new ParserSettings { AcceptUnknownMembers = true, PermissiveParsing = true });
        var bundle = parser.Parse<Bundle>(request.BundleJson);

        var warnings = new List<string>();
        var importedCount = 0;

        foreach (var entry in bundle.Entry)
        {
            if (entry.Resource is null)
            {
                warnings.Add("Skipped entry with null resource.");
                continue;
            }

            switch (entry.Resource)
            {
                case Patient:
                case Observation:
                case DiagnosticReport:
                case MedicationRequest:
                    importedCount++;
                    break;
                default:
                    warnings.Add($"Unsupported resource type: {entry.Resource.TypeName}");
                    break;
            }
        }

        await System.Threading.Tasks.Task.CompletedTask;

        return new ImportClinicalDataResult(importedCount, warnings);
    }
}
