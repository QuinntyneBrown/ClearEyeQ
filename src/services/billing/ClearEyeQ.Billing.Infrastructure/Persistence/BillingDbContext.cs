using ClearEyeQ.Billing.Domain.Aggregates;
using ClearEyeQ.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClearEyeQ.Billing.Infrastructure.Persistence;

public sealed class BillingDbContext : DbContext
{
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public BillingDbContext(DbContextOptions<BillingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.ToTable("subscriptions");
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Id)
                .HasColumnName("id");

            entity.Property(s => s.PlanTier)
                .HasColumnName("plan_tier")
                .HasConversion<int>();

            entity.Property(s => s.Status)
                .HasColumnName("status")
                .HasConversion<int>();

            entity.Property(s => s.StripeSubscriptionId)
                .HasColumnName("stripe_subscription_id")
                .HasMaxLength(256);

            entity.Property(s => s.CurrentPeriodStart)
                .HasColumnName("current_period_start");

            entity.Property(s => s.CurrentPeriodEnd)
                .HasColumnName("current_period_end");

            entity.Property(s => s.PaymentFailureCount)
                .HasColumnName("payment_failure_count");

            entity.ComplexProperty(s => s.TenantId, b =>
            {
                b.Property(t => t.Value)
                    .HasColumnName("tenant_id");
            });

            entity.ComplexProperty(s => s.PartitionKey, b =>
            {
                b.Property(p => p.Value)
                    .HasColumnName("partition_key")
                    .HasMaxLength(256);
            });

            entity.ComplexProperty(s => s.Audit, b =>
            {
                b.Property(a => a.CreatedAt).HasColumnName("created_at");
                b.Property(a => a.CreatedBy).HasColumnName("created_by").HasMaxLength(256);
                b.Property(a => a.ModifiedAt).HasColumnName("modified_at");
                b.Property(a => a.ModifiedBy).HasColumnName("modified_by").HasMaxLength(256);
            });

            entity.OwnsOne(s => s.UsageMeter, meter =>
            {
                meter.Property(m => m.MeterId).HasColumnName("meter_id");
                meter.Property(m => m.ScanCount).HasColumnName("scan_count");
                meter.Property(m => m.ScanLimit).HasColumnName("scan_limit");
                meter.Property(m => m.PeriodStart).HasColumnName("meter_period_start");
            });

            entity.Ignore(s => s.DomainEvents);
        });
    }
}
