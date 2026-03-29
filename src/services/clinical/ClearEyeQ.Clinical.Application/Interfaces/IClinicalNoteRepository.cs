namespace ClearEyeQ.Clinical.Application.Interfaces;

/// <summary>
/// Repository for managing clinician-authored notes attached to patient encounters.
/// </summary>
public interface IClinicalNoteRepository
{
    Task<Guid> CreateAsync(Guid tenantId, Guid patientId, string clinicianId, string content, CancellationToken cancellationToken = default);
}
