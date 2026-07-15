using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence.Configurations;

public sealed class AiUsageConfiguration : IEntityTypeConfiguration<AiUsage>
{
    public void Configure(EntityTypeBuilder<AiUsage> builder)
    {
        builder.ToTable("ai_usages");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(u => u.Period)
            .HasColumnName("period")
            .HasMaxLength(7)
            .IsRequired();

        builder.Property(u => u.CallCount)
            .HasColumnName("call_count")
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        // Um contador por usuário por mês.
        builder.HasIndex(u => new { u.UserId, u.Period })
            .IsUnique();
    }
}
