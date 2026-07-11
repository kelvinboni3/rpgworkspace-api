using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence.Configurations;

public sealed class BookVolumeConfiguration : IEntityTypeConfiguration<BookVolume>
{
    public void Configure(EntityTypeBuilder<BookVolume> builder)
    {
        builder.ToTable("book_volumes");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasColumnName("id");

        builder.Property(v => v.CharacterTabBlockId)
            .HasColumnName("character_tab_block_id")
            .IsRequired();

        builder.Property(v => v.Order)
            .HasColumnName("order")
            .IsRequired();

        builder.Property(v => v.MediaAssetId)
            .HasColumnName("media_asset_id")
            .IsRequired();

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(v => v.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne<CharacterTabBlock>()
            .WithMany()
            .HasForeignKey(v => v.CharacterTabBlockId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.MediaAsset)
            .WithMany()
            .HasForeignKey(v => v.MediaAssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
