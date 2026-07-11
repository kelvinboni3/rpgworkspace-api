using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence.Configurations;

public sealed class CharacterTabBlockLinkConfiguration : IEntityTypeConfiguration<CharacterTabBlockLink>
{
    public void Configure(EntityTypeBuilder<CharacterTabBlockLink> builder)
    {
        builder.ToTable("character_tab_block_links");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.SourceBlockId)
            .HasColumnName("source_block_id")
            .IsRequired();

        builder.Property(x => x.TargetBlockId)
            .HasColumnName("target_block_id")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(x => new { x.SourceBlockId, x.TargetBlockId }).IsUnique();
        builder.HasIndex(x => x.TargetBlockId);

        builder.HasOne<CharacterTabBlock>()
            .WithMany()
            .HasForeignKey(x => x.SourceBlockId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<CharacterTabBlock>()
            .WithMany()
            .HasForeignKey(x => x.TargetBlockId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
