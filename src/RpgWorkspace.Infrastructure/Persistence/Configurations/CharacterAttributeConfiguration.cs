using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence.Configurations;

public sealed class CharacterAttributeConfiguration : IEntityTypeConfiguration<CharacterAttribute>
{
    public void Configure(EntityTypeBuilder<CharacterAttribute> builder)
    {
        builder.ToTable("character_attributes");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id");

        builder.Property(a => a.CharacterId)
            .HasColumnName("character_id")
            .IsRequired();

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Value)
            .HasColumnName("value")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
