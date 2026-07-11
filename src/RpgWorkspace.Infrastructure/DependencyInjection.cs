using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Application.Services;
using RpgWorkspace.Infrastructure.Configuration;
using RpgWorkspace.Infrastructure.Persistence;
using RpgWorkspace.Infrastructure.Repositories;
using RpgWorkspace.Infrastructure.Security;
using RpgWorkspace.Infrastructure.Services;

namespace RpgWorkspace.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
            )
        );

        // JWT Settings
        services.Configure<JwtSettings>(options =>
            configuration.GetSection(JwtSettings.SectionName).Bind(options)
        );

        // Anthropic Settings
        services.Configure<AnthropicSettings>(options =>
            configuration.GetSection(AnthropicSettings.SectionName).Bind(options)
        );

        // Services
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        services.AddScoped<IWorkspaceService, WorkspaceService>();
        services.AddScoped<IWorkspaceMemberRepository, WorkspaceMemberRepository>();
        services.AddScoped<IWorkspaceMemberService, WorkspaceMemberService>();
        services.AddScoped<IWorkspaceInviteRepository, WorkspaceInviteRepository>();
        services.AddScoped<IWorkspaceInviteService, WorkspaceInviteService>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<INpcRepository, NpcRepository>();
        services.AddScoped<INpcService, NpcService>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IQuestRepository, QuestRepository>();
        services.AddScoped<IQuestService, QuestService>();
        services.AddScoped<ICharacterRepository, CharacterRepository>();
        services.AddScoped<ICharacterService, CharacterService>();
        services.AddScoped<ICharacterTabRepository, CharacterTabRepository>();
        services.AddScoped<ICharacterTabService, CharacterTabService>();
        services.AddScoped<ICharacterTabBlockRepository, CharacterTabBlockRepository>();
        services.AddScoped<ICharacterTabBlockService, CharacterTabBlockService>();
        services.AddScoped<IScheduleEventRepository, ScheduleEventRepository>();
        services.AddScoped<IScheduleResponseRepository, ScheduleResponseRepository>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IWikiPageRepository, WikiPageRepository>();
        services.AddScoped<IWikiPageService, WikiPageService>();
        services.AddScoped<IWorldLibraryItemRepository, WorldLibraryItemRepository>();
        services.AddScoped<IWorldLibraryItemService, WorldLibraryItemService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IDashboardService, DashboardService>();

        services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
        services.AddScoped<IBookVolumeRepository, BookVolumeRepository>();
        services.AddScoped<IBookVolumeService, BookVolumeService>();

        services.AddScoped<INoteStructuringGateway, AnthropicNoteStructuringGateway>();
        services.AddScoped<INoteStructuringService, NoteStructuringService>();

        return services;
    }
}
