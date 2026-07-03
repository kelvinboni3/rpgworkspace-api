using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence.Configurations;

public sealed class ScheduleResponseConfiguration : IEntityTypeConfiguration<ScheduleResponse>
{
    public void Configure(EntityTypeBuilder<ScheduleResponse> builder)
    {
        builder.ToTable("schedule_responses");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id");

        builder.Property(r => r.ScheduleEventId)
            .HasColumnName("schedule_event_id")
            .IsRequired();

        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.Response)
            .HasColumnName("response")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasColumnName("comment")
            .HasMaxLength(500);

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(r => new { r.ScheduleEventId, r.UserId })
            .IsUnique();

        builder.HasOne(r => r.User)
            .WithMany(u => u.ScheduleResponses)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
