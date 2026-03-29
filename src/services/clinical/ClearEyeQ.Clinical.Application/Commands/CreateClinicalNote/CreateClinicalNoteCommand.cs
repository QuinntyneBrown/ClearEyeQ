using MediatR;

namespace ClearEyeQ.Clinical.Application.Commands.CreateClinicalNote;

public sealed record CreateClinicalNoteCommand(
    Guid TenantId,
    Guid PatientId,
    string ClinicianId,
    string Content) : IRequest<Guid>;
