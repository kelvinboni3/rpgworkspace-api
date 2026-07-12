using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(s => s.UserId)
            .IsUnique();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.Plan)
            .HasColumnName("plan")
            .HasMaxLength(50);

        builder.Property(s => s.GatewayCustomerId)
            .HasColumnName("gateway_customer_id")
            .HasMaxLength(200);

        builder.Property(s => s.GatewaySubscriptionId)
            .HasColumnName("gateway_subscription_id")
            .HasMaxLength(200);

        builder.HasIndex(s => s.GatewayCustomerId);

        builder.Property(s => s.CurrentPeriodEnd)
            .HasColumnName("current_period_end");

        builder.Property(s => s.ManualOverride)
            .HasColumnName("manual_override")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
