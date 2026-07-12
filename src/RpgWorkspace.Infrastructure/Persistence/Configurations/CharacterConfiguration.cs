using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence.Configurations;

public sealed class CharacterConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> builder)
    {
        builder.ToTable("characters");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.CampaignId)
            .HasColumnName("campaign_id");

        builder.Property(c => c.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(c => c.Race)
            .HasColumnName("race")
            .HasMaxLength(100);

        builder.Property(c => c.Class)
            .HasColumnName("class")
            .HasMaxLength(100);

        builder.Property(c => c.Level)
            .HasColumnName("level")
            .IsRequired();

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.PortraitAssetId)
            .HasColumnName("portrait_asset_id");

        builder.HasOne<MediaAsset>()
            .WithMany()
            .HasForeignKey(c => c.PortraitAssetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(c => c.HpCurrent)
            .HasColumnName("hp_current");

        builder.Property(c => c.HpMax)
            .HasColumnName("hp_max");

        builder.Property(c => c.MpCurrent)
            .HasColumnName("mp_current");

        builder.Property(c => c.MpMax)
            .HasColumnName("mp_max");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
