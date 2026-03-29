using ClearEyeQ.Clinical.Application.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace ClearEyeQ.Clinical.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for the Clinical Portal read model store.
/// Contains projected read model tables and referral/note entities.
/// </summary>
public sealed class ClinicalDbContext : DbContext
{
    public ClinicalDbContext(DbContextOptions<ClinicalDbContext> options) : base(options)
    {
    }

    public DbSet<PatientSummaryReadModel> PatientSummaries => Set<PatientSummaryReadModel>();
    public DbSet<ScanResultReadModel> ScanResults => Set<ScanResultReadModel>();
    public DbSet<DiagnosticReadModel> Diagnostics => Set<DiagnosticReadModel>();
    public DbSet<TreatmentPlanReadModel> TreatmentPlans => Set<TreatmentPlanReadModel>();
    public DbSet<ReferralEntity> Referrals => Set<ReferralEntity>();
    public DbSet<ClinicalNoteEntity> ClinicalNotes => Set<ClinicalNoteEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientSummaryReadModel>(e =>
        {
            e.ToTable("patient_summaries");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.PatientId }).IsUnique();
            e.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<ScanResultReadModel>(e =>
        {
            e.ToTable("scan_results");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.PatientId });
            e.HasIndex(x => new { x.TenantId, x.ScanId }).IsUnique();
        });

        modelBuilder.Entity<DiagnosticReadModel>(e =>
        {
            e.ToTable("diagnostics");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.PatientId });
            e.HasIndex(x => new { x.TenantId, x.DiagnosticSessionId }).IsUnique();
        });

        modelBuilder.Entity<TreatmentPlanReadModel>(e =>
        {
            e.ToTable("treatment_plans");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.PatientId });
            e.HasIndex(x => new { x.TenantId, x.TreatmentPlanId }).IsUnique();
        });

        modelBuilder.Entity<ReferralEntity>(e =>
        {
            e.ToTable("referrals");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Status });
        });

        modelBuilder.Entity<ClinicalNoteEntity>(e =>
        {
            e.ToTable("clinical_notes");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.PatientId });
        });
    }
}

public sealed class ReferralEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? ClinicianId { get; set; }
    public string? Rationale { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ResolvedAtUtc { get; set; }
}

public sealed class ClinicalNoteEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PatientId { get; set; }
    public string ClinicianId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
}
