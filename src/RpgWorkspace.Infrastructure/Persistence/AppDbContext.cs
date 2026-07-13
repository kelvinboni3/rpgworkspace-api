using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Npc> Npcs => Set<Npc>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Quest> Quests => Set<Quest>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<ScheduleEvent> ScheduleEvents => Set<ScheduleEvent>();
    public DbSet<ScheduleResponse> ScheduleResponses => Set<ScheduleResponse>();
    public DbSet<WikiPage> WikiPages => Set<WikiPage>();
    public DbSet<WorldLibraryItem> WorldLibraryItems => Set<WorldLibraryItem>();
    public DbSet<WorkspaceInvite> WorkspaceInvites => Set<WorkspaceInvite>();
    public DbSet<CharacterTab> CharacterTabs => Set<CharacterTab>();
    public DbSet<CharacterTabBlock> CharacterTabBlocks => Set<CharacterTabBlock>();
    public DbSet<CharacterTabBlockLink> CharacterTabBlockLinks => Set<CharacterTabBlockLink>();
    public DbSet<BookVolume> BookVolumes => Set<BookVolume>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<SessionTag> SessionTags => Set<SessionTag>();
    public DbSet<NpcTag> NpcTags => Set<NpcTag>();
    public DbSet<LocationTag> LocationTags => Set<LocationTag>();
    public DbSet<QuestTag> QuestTags => Set<QuestTag>();
    public DbSet<WikiPageTag> WikiPageTags => Set<WikiPageTag>();
    public DbSet<WorldLibraryItemTag> WorldLibraryItemTags => Set<WorldLibraryItemTag>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica automaticamente todas as configurações IEntityTypeConfiguration<T>
        // encontradas no assembly desta camada.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
