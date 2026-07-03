using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence.Configurations;

public sealed class NarrativeItemConfiguration : IEntityTypeConfiguration<NarrativeItem>
{
    public void Configure(EntityTypeBuilder<NarrativeItem> builder)
    {
        builder.ToTable("narrative_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id");

        builder.Property(i => i.CharacterId)
            .HasColumnName("character_id")
            .IsRequired();

        builder.Property(i => i.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(i => i.Origin)
            .HasColumnName("origin")
            .HasMaxLength(500);

        builder.Property(i => i.SessionId)
            .HasColumnName("session_id");

        builder.Property(i => i.Importance)
            .HasColumnName("importance")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(i => i.Session)
            .WithMany()
            .HasForeignKey(i => i.SessionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
