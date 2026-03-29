using MediatR;

namespace ClearEyeQ.Fhir.Application.Commands.ExportPatientData;

public sealed record ExportPatientDataCommand(
    Guid TenantId,
    Guid PatientId) : IRequest<ExportPatientDataResult>;

public sealed record ExportPatientDataResult(
    string BlobName,
    int ResourceCount);
