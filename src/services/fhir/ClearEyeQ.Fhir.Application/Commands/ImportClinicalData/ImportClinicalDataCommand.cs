using MediatR;

namespace ClearEyeQ.Fhir.Application.Commands.ImportClinicalData;

public sealed record ImportClinicalDataCommand(
    Guid TenantId,
    string BundleJson) : IRequest<ImportClinicalDataResult>;

public sealed record ImportClinicalDataResult(
    int ImportedResourceCount,
    IReadOnlyList<string> Warnings);
