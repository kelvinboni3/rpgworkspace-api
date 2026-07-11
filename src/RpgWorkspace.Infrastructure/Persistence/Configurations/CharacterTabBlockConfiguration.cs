using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence.Configurations;

public sealed class CharacterTabBlockConfiguration : IEntityTypeConfiguration<CharacterTabBlock>
{
    public void Configure(EntityTypeBuilder<CharacterTabBlock> builder)
    {
        builder.ToTable("character_tab_blocks");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnName("id");

        builder.Property(b => b.CharacterTabId)
            .HasColumnName("character_tab_id")
            .IsRequired();

        builder.Property(b => b.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(b => b.Order)
            .HasColumnName("order")
            .IsRequired();

        builder.Property(b => b.Title)
            .HasColumnName("title")
            .HasMaxLength(200);

        builder.Property(b => b.Meta)
            .HasColumnName("meta")
            .HasMaxLength(200);

        builder.Property(b => b.Content)
            .HasColumnName("content")
            .HasColumnType("text");

        builder.Property(b => b.PayloadJson)
            .HasColumnName("payload_json")
            .HasColumnType("text");

        builder.Property(b => b.ParentBlockId)
            .HasColumnName("parent_block_id");

        builder.Property(b => b.AccentColor)
            .HasColumnName("accent_color")
            .HasMaxLength(20);

        builder.Property(b => b.ImageAssetId)
            .HasColumnName("image_asset_id");

        builder.HasOne<MediaAsset>()
            .WithMany()
            .HasForeignKey(b => b.ImageAssetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(b => b.CharacterTab)
            .WithMany(t => t.Blocks)
            .HasForeignKey(b => b.CharacterTabId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<CharacterTabBlock>()
            .WithMany(b => b.Children)
            .HasForeignKey(b => b.ParentBlockId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(CharacterTabBlock.Children))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
