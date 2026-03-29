using ClearEyeQ.Clinical.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.Events;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Clinical.Application.Commands.CreateClinicalNote;

public sealed class CreateClinicalNoteHandler : IRequestHandler<CreateClinicalNoteCommand, Guid>
{
    private readonly IClinicalNoteRepository _noteRepository;
    private readonly IIntegrationEventPublisher _eventPublisher;

    public CreateClinicalNoteHandler(
        IClinicalNoteRepository noteRepository,
        IIntegrationEventPublisher eventPublisher)
    {
        _noteRepository = noteRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<Guid> Handle(CreateClinicalNoteCommand request, CancellationToken cancellationToken)
    {
        var noteId = await _noteRepository.CreateAsync(
            request.TenantId,
            request.PatientId,
            request.ClinicianId,
            request.Content,
            cancellationToken);

        var payload = new ClinicalNoteAddedPayload(
            noteId,
            request.PatientId,
            request.ClinicianId,
            DateTimeOffset.UtcNow);

        var envelope = IntegrationEventEnvelope.Create(
            payload,
            new TenantId(request.TenantId),
            request.PatientId,
            Guid.NewGuid(),
            Guid.NewGuid());

        await _eventPublisher.PublishAsync(envelope, cancellationToken);

        return noteId;
    }
}

internal sealed record ClinicalNoteAddedPayload(
    Guid NoteId,
    Guid PatientId,
    string ClinicianId,
    DateTimeOffset CreatedAtUtc);
