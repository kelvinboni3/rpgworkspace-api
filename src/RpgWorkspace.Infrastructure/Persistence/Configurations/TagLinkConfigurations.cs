using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence.Configurations;

public sealed class SessionTagConfiguration : IEntityTypeConfiguration<SessionTag>
{
    public void Configure(EntityTypeBuilder<SessionTag> builder) { builder.ToTable("session_tags"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).HasColumnName("id"); builder.Property(x => x.SessionId).HasColumnName("session_id").IsRequired(); builder.Property(x => x.TagId).HasColumnName("tag_id").IsRequired(); builder.Ignore(x => x.CreatedAt); builder.Ignore(x => x.UpdatedAt); builder.HasIndex(x => new { x.SessionId, x.TagId }).IsUnique(); builder.HasOne<Session>().WithMany().HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade); }
}

public sealed class NpcTagConfiguration : IEntityTypeConfiguration<NpcTag>
{
    public void Configure(EntityTypeBuilder<NpcTag> builder) { builder.ToTable("npc_tags"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).HasColumnName("id"); builder.Property(x => x.NpcId).HasColumnName("npc_id").IsRequired(); builder.Property(x => x.TagId).HasColumnName("tag_id").IsRequired(); builder.Ignore(x => x.CreatedAt); builder.Ignore(x => x.UpdatedAt); builder.HasIndex(x => new { x.NpcId, x.TagId }).IsUnique(); builder.HasOne<Npc>().WithMany().HasForeignKey(x => x.NpcId).OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade); }
}

public sealed class LocationTagConfiguration : IEntityTypeConfiguration<LocationTag>
{
    public void Configure(EntityTypeBuilder<LocationTag> builder) { builder.ToTable("location_tags"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).HasColumnName("id"); builder.Property(x => x.LocationId).HasColumnName("location_id").IsRequired(); builder.Property(x => x.TagId).HasColumnName("tag_id").IsRequired(); builder.Ignore(x => x.CreatedAt); builder.Ignore(x => x.UpdatedAt); builder.HasIndex(x => new { x.LocationId, x.TagId }).IsUnique(); builder.HasOne<Location>().WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade); }
}

public sealed class QuestTagConfiguration : IEntityTypeConfiguration<QuestTag>
{
    public void Configure(EntityTypeBuilder<QuestTag> builder) { builder.ToTable("quest_tags"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).HasColumnName("id"); builder.Property(x => x.QuestId).HasColumnName("quest_id").IsRequired(); builder.Property(x => x.TagId).HasColumnName("tag_id").IsRequired(); builder.Ignore(x => x.CreatedAt); builder.Ignore(x => x.UpdatedAt); builder.HasIndex(x => new { x.QuestId, x.TagId }).IsUnique(); builder.HasOne<Quest>().WithMany().HasForeignKey(x => x.QuestId).OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade); }
}

public sealed class WikiPageTagConfiguration : IEntityTypeConfiguration<WikiPageTag>
{
    public void Configure(EntityTypeBuilder<WikiPageTag> builder) { builder.ToTable("wiki_page_tags"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).HasColumnName("id"); builder.Property(x => x.WikiPageId).HasColumnName("wiki_page_id").IsRequired(); builder.Property(x => x.TagId).HasColumnName("tag_id").IsRequired(); builder.Ignore(x => x.CreatedAt); builder.Ignore(x => x.UpdatedAt); builder.HasIndex(x => new { x.WikiPageId, x.TagId }).IsUnique(); builder.HasOne<WikiPage>().WithMany().HasForeignKey(x => x.WikiPageId).OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade); }
}

public sealed class WorldLibraryItemTagConfiguration : IEntityTypeConfiguration<WorldLibraryItemTag>
{
    public void Configure(EntityTypeBuilder<WorldLibraryItemTag> builder) { builder.ToTable("world_library_item_tags"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).HasColumnName("id"); builder.Property(x => x.WorldLibraryItemId).HasColumnName("world_library_item_id").IsRequired(); builder.Property(x => x.TagId).HasColumnName("tag_id").IsRequired(); builder.Ignore(x => x.CreatedAt); builder.Ignore(x => x.UpdatedAt); builder.HasIndex(x => new { x.WorldLibraryItemId, x.TagId }).IsUnique(); builder.HasOne<WorldLibraryItem>().WithMany().HasForeignKey(x => x.WorldLibraryItemId).OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade); }
}

public sealed class PlayerNoteTagConfiguration : IEntityTypeConfiguration<PlayerNoteTag>
{
    public void Configure(EntityTypeBuilder<PlayerNoteTag> builder) { builder.ToTable("player_note_tags"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).HasColumnName("id"); builder.Property(x => x.PlayerNoteId).HasColumnName("player_note_id").IsRequired(); builder.Property(x => x.TagId).HasColumnName("tag_id").IsRequired(); builder.Ignore(x => x.CreatedAt); builder.Ignore(x => x.UpdatedAt); builder.HasIndex(x => new { x.PlayerNoteId, x.TagId }).IsUnique(); builder.HasOne<PlayerNote>().WithMany().HasForeignKey(x => x.PlayerNoteId).OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade); }
}

public sealed class TheoryTagConfiguration : IEntityTypeConfiguration<TheoryTag>
{
    public void Configure(EntityTypeBuilder<TheoryTag> builder) { builder.ToTable("theory_tags"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).HasColumnName("id"); builder.Property(x => x.TheoryId).HasColumnName("theory_id").IsRequired(); builder.Property(x => x.TagId).HasColumnName("tag_id").IsRequired(); builder.Ignore(x => x.CreatedAt); builder.Ignore(x => x.UpdatedAt); builder.HasIndex(x => new { x.TheoryId, x.TagId }).IsUnique(); builder.HasOne<Theory>().WithMany().HasForeignKey(x => x.TheoryId).OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade); }
}

public sealed class OperationTagConfiguration : IEntityTypeConfiguration<OperationTag>
{
    public void Configure(EntityTypeBuilder<OperationTag> builder) { builder.ToTable("operation_tags"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).HasColumnName("id"); builder.Property(x => x.OperationId).HasColumnName("operation_id").IsRequired(); builder.Property(x => x.TagId).HasColumnName("tag_id").IsRequired(); builder.Ignore(x => x.CreatedAt); builder.Ignore(x => x.UpdatedAt); builder.HasIndex(x => new { x.OperationId, x.TagId }).IsUnique(); builder.HasOne<Operation>().WithMany().HasForeignKey(x => x.OperationId).OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade); }
}

public sealed class NarrativeItemTagConfiguration : IEntityTypeConfiguration<NarrativeItemTag>
{
    public void Configure(EntityTypeBuilder<NarrativeItemTag> builder) { builder.ToTable("narrative_item_tags"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).HasColumnName("id"); builder.Property(x => x.NarrativeItemId).HasColumnName("narrative_item_id").IsRequired(); builder.Property(x => x.TagId).HasColumnName("tag_id").IsRequired(); builder.Ignore(x => x.CreatedAt); builder.Ignore(x => x.UpdatedAt); builder.HasIndex(x => new { x.NarrativeItemId, x.TagId }).IsUnique(); builder.HasOne<NarrativeItem>().WithMany().HasForeignKey(x => x.NarrativeItemId).OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade); }
}
