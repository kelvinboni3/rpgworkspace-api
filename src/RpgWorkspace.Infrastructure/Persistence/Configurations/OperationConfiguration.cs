using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence.Configurations;

public sealed class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        builder.ToTable("operations");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id");

        builder.Property(o => o.CharacterId)
            .HasColumnName("character_id")
            .IsRequired();

        builder.Property(o => o.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(o => o.Objective)
            .HasColumnName("objective")
            .HasMaxLength(1000);

        builder.Property(o => o.Plan)
            .HasColumnName("plan")
            .HasMaxLength(3000);

        builder.Property(o => o.RequiredResources)
            .HasColumnName("required_resources")
            .HasMaxLength(2000);

        builder.Property(o => o.Risks)
            .HasColumnName("risks")
            .HasMaxLength(2000);

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.Result)
            .HasColumnName("result")
            .HasMaxLength(2000);

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
