using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence.Configurations;

public sealed class CharacterTabEntryConfiguration : IEntityTypeConfiguration<CharacterTabEntry>
{
    public void Configure(EntityTypeBuilder<CharacterTabEntry> builder)
    {
        builder.ToTable("character_tab_entries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.CharacterTabId)
            .HasColumnName("character_tab_id")
            .IsRequired();

        builder.Property(e => e.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Content)
            .HasColumnName("content")
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
