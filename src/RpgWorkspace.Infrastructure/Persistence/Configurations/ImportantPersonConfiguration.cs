using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence.Configurations;

public sealed class ImportantPersonConfiguration : IEntityTypeConfiguration<ImportantPerson>
{
    public void Configure(EntityTypeBuilder<ImportantPerson> builder)
    {
        builder.ToTable("important_people");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.CharacterId)
            .HasColumnName("character_id")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(p => p.FirstImpression)
            .HasColumnName("first_impression")
            .HasMaxLength(1000);

        builder.Property(p => p.Analysis)
            .HasColumnName("analysis")
            .HasMaxLength(2000);

        builder.Property(p => p.TrustLevel)
            .HasColumnName("trust_level")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.RiskLevel)
            .HasColumnName("risk_level")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.UtilityLevel)
            .HasColumnName("utility_level")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.Property(p => p.LastContactAt)
            .HasColumnName("last_contact_at");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
