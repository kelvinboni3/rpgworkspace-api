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
            .HasColumnName("campaign_id")
            .IsRequired();

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

        builder.Property(c => c.PortraitUrl)
            .HasColumnName("portrait_url")
            .HasColumnType("text");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasMany(c => c.PlayerNotes)
            .WithOne(n => n.Character)
            .HasForeignKey(n => n.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Character.PlayerNotes))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.ImportantPeople)
            .WithOne(p => p.Character)
            .HasForeignKey(p => p.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Character.ImportantPeople))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.Theories)
            .WithOne(t => t.Character)
            .HasForeignKey(t => t.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Character.Theories))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.Operations)
            .WithOne(o => o.Character)
            .HasForeignKey(o => o.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Character.Operations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.NarrativeItems)
            .WithOne(i => i.Character)
            .HasForeignKey(i => i.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Character.NarrativeItems))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
