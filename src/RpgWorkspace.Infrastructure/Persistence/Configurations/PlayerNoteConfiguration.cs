using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence.Configurations;

public sealed class PlayerNoteConfiguration : IEntityTypeConfiguration<PlayerNote>
{
    public void Configure(EntityTypeBuilder<PlayerNote> builder)
    {
        builder.ToTable("player_notes");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasColumnName("id");

        builder.Property(n => n.CharacterId)
            .HasColumnName("character_id")
            .IsRequired();

        builder.Property(n => n.SessionId)
            .HasColumnName("session_id");

        builder.Property(n => n.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.Content)
            .HasColumnName("content")
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(n => n.Tags)
            .HasColumnName("tags")
            .HasMaxLength(500);

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(n => n.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(n => n.Session)
            .WithMany()
            .HasForeignKey(n => n.SessionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
