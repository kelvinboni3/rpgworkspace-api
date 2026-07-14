using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence.Configurations;

public sealed class CharacterTabConfiguration : IEntityTypeConfiguration<CharacterTab>
{
    public void Configure(EntityTypeBuilder<CharacterTab> builder)
    {
        builder.ToTable("character_tabs");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.CharacterId)
            .HasColumnName("character_id")
            .IsRequired();

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Order)
            .HasColumnName("order")
            .IsRequired();

        builder.Property(t => t.IsPublic)
            .HasColumnName("is_public")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Metadata
            .FindNavigation(nameof(CharacterTab.Blocks))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
