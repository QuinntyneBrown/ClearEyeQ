using ClearEyeQ.Clinical.Application.Interfaces;

namespace ClearEyeQ.Clinical.Infrastructure.Persistence;

public sealed class EfClinicalNoteRepository : IClinicalNoteRepository
{
    private readonly ClinicalDbContext _db;

    public EfClinicalNoteRepository(ClinicalDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> CreateAsync(Guid tenantId, Guid patientId, string clinicianId, string content, CancellationToken cancellationToken = default)
    {
        var entity = new ClinicalNoteEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PatientId = patientId,
            ClinicianId = clinicianId,
            Content = content,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        _db.ClinicalNotes.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
