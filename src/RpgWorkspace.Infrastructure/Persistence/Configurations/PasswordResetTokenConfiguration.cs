using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence.Configurations;

public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(t => t.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(t => t.TokenHash)
            .IsUnique();

        builder.Property(t => t.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(t => t.UsedAt)
            .HasColumnName("used_at");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
