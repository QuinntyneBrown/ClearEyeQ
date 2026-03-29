using ClearEyeQ.Notifications.Domain.Aggregates;
using ClearEyeQ.Notifications.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClearEyeQ.Notifications.Infrastructure.Persistence;

public sealed class NotificationDbContext : DbContext
{
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> Preferences => Set<NotificationPreference>();

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(n => n.Id);

            entity.Property(n => n.Id).HasColumnName("id");
            entity.Property(n => n.Category).HasColumnName("category").HasConversion<int>();
            entity.Property(n => n.Channel).HasColumnName("channel").HasConversion<int>();
            entity.Property(n => n.Status).HasColumnName("status").HasConversion<int>();
            entity.Property(n => n.CreatedAt).HasColumnName("created_at");

            entity.ComplexProperty(n => n.UserId, b =>
            {
                b.Property(u => u.Value).HasColumnName("user_id");
            });

            entity.ComplexProperty(n => n.TenantId, b =>
            {
                b.Property(t => t.Value).HasColumnName("tenant_id");
            });

            entity.ComplexProperty(n => n.PartitionKey, b =>
            {
                b.Property(p => p.Value).HasColumnName("partition_key").HasMaxLength(256);
            });

            entity.ComplexProperty(n => n.Content, b =>
            {
                b.Property(c => c.Title).HasColumnName("title").HasMaxLength(256);
                b.Property(c => c.Body).HasColumnName("body").HasMaxLength(4096);
                b.Property(c => c.ActionUrl).HasColumnName("action_url").HasMaxLength(2048);
                b.Ignore(c => c.Data);
            });

            entity.ComplexProperty(n => n.Audit, b =>
            {
                b.Property(a => a.CreatedAt).HasColumnName("audit_created_at");
                b.Property(a => a.CreatedBy).HasColumnName("audit_created_by").HasMaxLength(256);
                b.Property(a => a.ModifiedAt).HasColumnName("audit_modified_at");
                b.Property(a => a.ModifiedBy).HasColumnName("audit_modified_by").HasMaxLength(256);
            });

            entity.OwnsMany(n => n.DeliveryAttempts, attempt =>
            {
                attempt.ToTable("delivery_attempts");
                attempt.WithOwner().HasForeignKey("notification_id");
                attempt.HasKey(a => a.AttemptId);
                attempt.Property(a => a.AttemptId).HasColumnName("attempt_id");
                attempt.Property(a => a.Channel).HasColumnName("channel").HasConversion<int>();
                attempt.Property(a => a.Status).HasColumnName("status").HasConversion<int>();
                attempt.Property(a => a.AttemptedAt).HasColumnName("attempted_at");
                attempt.Property(a => a.ErrorMessage).HasColumnName("error_message").HasMaxLength(1024);
            });

            entity.Ignore(n => n.DomainEvents);
        });

        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.ToTable("notification_preferences");
            entity.HasKey(p => p.PreferenceId);

            entity.Property(p => p.PreferenceId).HasColumnName("preference_id");
            entity.Property(p => p.Channel).HasColumnName("channel").HasConversion<int>();
            entity.Property(p => p.Enabled).HasColumnName("enabled");

            entity.ComplexProperty(p => p.UserId, b =>
            {
                b.Property(u => u.Value).HasColumnName("user_id");
            });

            entity.ComplexProperty(p => p.TenantId, b =>
            {
                b.Property(t => t.Value).HasColumnName("tenant_id");
            });

            entity.OwnsOne(p => p.QuietHoursPolicy, qh =>
            {
                qh.Property(q => q.Start).HasColumnName("quiet_hours_start");
                qh.Property(q => q.End).HasColumnName("quiet_hours_end");
                qh.Property(q => q.TimeZone).HasColumnName("quiet_hours_timezone").HasMaxLength(128);
            });
        });
    }
}
