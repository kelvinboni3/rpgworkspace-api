using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence.Configurations;

public sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("campaigns");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.WorkspaceId)
            .HasColumnName("workspace_id")
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(c => c.SystemName)
            .HasColumnName("system_name")
            .HasMaxLength(100);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasMany(c => c.Sessions)
            .WithOne(s => s.Campaign)
            .HasForeignKey(s => s.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Campaign.Sessions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.Npcs)
            .WithOne(n => n.Campaign)
            .HasForeignKey(n => n.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Campaign.Npcs))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.Locations)
            .WithOne(l => l.Campaign)
            .HasForeignKey(l => l.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Campaign.Locations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.Quests)
            .WithOne(q => q.Campaign)
            .HasForeignKey(q => q.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Campaign.Quests))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.Characters)
            .WithOne(ch => ch.Campaign)
            .HasForeignKey(ch => ch.CampaignId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Campaign.Characters))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.ScheduleEvents)
            .WithOne(e => e.Campaign)
            .HasForeignKey(e => e.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Campaign.ScheduleEvents))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.WikiPages)
            .WithOne(p => p.Campaign)
            .HasForeignKey(p => p.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Campaign.WikiPages))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.WorldLibraryItems)
            .WithOne(i => i.Campaign)
            .HasForeignKey(i => i.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Campaign.WorldLibraryItems))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
