using RpgWorkspace.Application.DTOs.Character;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.Services;

public sealed class CharacterService : ICharacterService
{
    private static readonly string[] DefaultTabNames =
        ["Status", "Diário", "Itens Narrativos", "Teorias", "Operações", "Pessoas"];

    private readonly ICharacterRepository _characterRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICharacterTabRepository _characterTabRepository;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IUnitOfWork _unitOfWork;

    public CharacterService(
        ICharacterRepository characterRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        ICharacterTabRepository characterTabRepository,
        IWorkspaceMemberRepository workspaceMemberRepository,
        IUserRepository userRepository,
        IMediaAssetRepository mediaAssetRepository,
        IPasswordHasher passwordHasher,
        ISubscriptionService subscriptionService,
        IUnitOfWork unitOfWork)
    {
        _characterRepository = characterRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _characterTabRepository = characterTabRepository;
        _workspaceMemberRepository = workspaceMemberRepository;
        _userRepository = userRepository;
        _mediaAssetRepository = mediaAssetRepository;
        _passwordHasher = passwordHasher;
        _subscriptionService = subscriptionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CharacterResponse>> GetAllByCampaignAsync(
        Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        var characters = await _characterRepository.GetAllByCampaignAsync(campaignId, cancellationToken);

        return characters.Select(ToResponse).ToList();
    }

    public async Task<CharacterResponse> GetByIdAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        EnsureCanViewCharacter(character, workspace, requestingUserId);

        return ToResponse(character);
    }

    public async Task<IReadOnlyList<CharacterResponse>> GetMineAsync(
        Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var characters = await _characterRepository.GetAllByUserAsync(requestingUserId, cancellationToken);
        return characters.Select(ToResponse).ToList();
    }

    public async Task<CharacterResponse> CreateSoloAsync(
        Guid requestingUserId, CreateSoloCharacterRequest request, CancellationToken cancellationToken = default)
    {
        await _subscriptionService.EnsureCanCreateCharacterAsync(requestingUserId, cancellationToken);

        var character = Character.Create(
            campaignId: null,
            requestingUserId,
            request.Name,
            request.Description,
            request.Race,
            request.Class,
            request.Level,
            request.Status);

        await _characterRepository.AddAsync(character, cancellationToken);

        for (var i = 0; i < DefaultTabNames.Length; i++)
        {
            var tab = CharacterTab.Create(character.Id, DefaultTabNames[i], i);
            await _characterTabRepository.AddAsync(tab, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(character);
    }

    public async Task<CharacterResponse> CreateAsync(
        Guid campaignId, CreateCharacterRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);
        EnsureCanCreateForUser(workspace, requestingUserId, request.UserId);

        var character = Character.Create(
            campaignId,
            request.UserId,
            request.Name,
            request.Description,
            request.Race,
            request.Class,
            request.Level,
            request.Status);

        await _characterRepository.AddAsync(character, cancellationToken);

        for (var i = 0; i < DefaultTabNames.Length; i++)
        {
            var tab = CharacterTab.Create(character.Id, DefaultTabNames[i], i);
            await _characterTabRepository.AddAsync(tab, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(character);
    }

    public async Task<CharacterWithAccountResponse> CreateWithAccountAsync(
        Guid campaignId, CreateCharacterWithAccountRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);

        if (!workspace.IsOwnerOrMaster(requestingUserId))
            throw new UnauthorizedAccessException("Only Owner or Master can create a character with a new login.");

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
            throw new InvalidOperationException("E-mail already in use.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.PlayerName, email, passwordHash);
        await _userRepository.AddAsync(user, cancellationToken);

        var member = workspace.AddMember(user.Id, WorkspaceRole.Player);
        await _workspaceMemberRepository.AddAsync(member, cancellationToken);

        var character = Character.Create(
            campaignId,
            user.Id,
            request.CharacterName,
            request.Description,
            request.Race,
            request.Class,
            request.Level,
            request.Status);

        await _characterRepository.AddAsync(character, cancellationToken);

        for (var i = 0; i < DefaultTabNames.Length; i++)
        {
            var tab = CharacterTab.Create(character.Id, DefaultTabNames[i], i);
            await _characterTabRepository.AddAsync(tab, cancellationToken);
        }

        user.SetDefaultCharacter(character.Id);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CharacterWithAccountResponse(ToResponse(character), user.Id.ToString(), user.Name, user.Email);
    }

    public async Task<CharacterResponse> UpdateAsync(
        Guid characterId, UpdateCharacterRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character not found.");

        character.Update(request.Name, request.Description, request.Race, request.Class, request.Level, request.Status);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(character);
    }

    public async Task<CharacterResponse> UploadPortraitAsync(
        Guid characterId, string originalFileName, string contentType, long fileSizeBytes, Stream content,
        Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character not found.");

        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only image files are accepted.");

        using var memoryStream = new MemoryStream();
        await content.CopyToAsync(memoryStream, cancellationToken);

        var previousAssetId = character.PortraitAssetId;

        var asset = MediaAsset.Create(memoryStream.ToArray(), contentType, originalFileName, fileSizeBytes);
        await _mediaAssetRepository.AddAsync(asset, cancellationToken);
        character.SetPortrait(asset.Id);

        if (previousAssetId.HasValue)
        {
            var previousAsset = await _mediaAssetRepository.GetByIdAsync(previousAssetId.Value, cancellationToken);
            if (previousAsset is not null)
                _mediaAssetRepository.Remove(previousAsset);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(character);
    }

    public async Task<CharacterResponse> RemovePortraitAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character not found.");

        if (character.PortraitAssetId.HasValue)
        {
            var previousAsset = await _mediaAssetRepository.GetByIdAsync(character.PortraitAssetId.Value, cancellationToken);
            if (previousAsset is not null)
                _mediaAssetRepository.Remove(previousAsset);
        }

        character.SetPortrait(null);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(character);
    }

    public async Task<(byte[] Content, string ContentType)> GetPortraitContentAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        EnsureCanViewCharacter(character, workspace, requestingUserId);

        if (!character.PortraitAssetId.HasValue)
            throw new KeyNotFoundException("Character has no portrait.");

        var asset = await _mediaAssetRepository.GetByIdAsync(character.PortraitAssetId.Value, cancellationToken)
            ?? throw new KeyNotFoundException("Portrait not found.");

        return (asset.Content, asset.ContentType);
    }

    public async Task<CharacterResponse> UpdateVitalsAsync(
        Guid characterId, UpdateCharacterVitalsRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character not found.");

        character.UpdateVitals(request.HpCurrent, request.HpMax, request.MpCurrent, request.MpMax);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(character);
    }

    public async Task<CharacterResponse> UpdateAccentColorAsync(
        Guid characterId, UpdateCharacterAccentColorRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character not found.");

        character.SetAccentColor(NormalizeAccentColor(request.AccentColor));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(character);
    }

    private static readonly HashSet<string> AllowedAccentColors = new(StringComparer.OrdinalIgnoreCase)
    {
        "gold", "crimson", "violet",
    };

    private static string? NormalizeAccentColor(string? accentColor)
    {
        if (string.IsNullOrWhiteSpace(accentColor))
            return null;

        if (!AllowedAccentColors.Contains(accentColor))
            throw new ArgumentException($"Invalid accent color: {accentColor}.");

        return accentColor.ToLowerInvariant();
    }

    public async Task<CharacterResponse> EnableSharingAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character not found.");

        if (!character.IsShared)
        {
            character.EnableSharing(GenerateShareToken());
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return ToResponse(character);
    }

    public async Task<CharacterResponse> DisableSharingAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character not found.");

        if (character.IsShared)
        {
            character.DisableSharing();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return ToResponse(character);
    }

    // URL-safe token (base64url without padding) — 16 random bytes → ~22 chars, fits the 32-char column.
    private static string GenerateShareToken()
    {
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public async Task DeleteAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Character not found.");

        _characterRepository.Remove(character);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Character> GetCharacterOrThrowAsync(Guid id, CancellationToken ct)
        => await _characterRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Character not found.");

    private async Task<Campaign> GetCampaignOrThrowAsync(Guid id, CancellationToken ct)
        => await _campaignRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Campaign not found.");

    private async Task<Workspace> GetWorkspaceWithMembersOrThrowAsync(Guid id, CancellationToken ct)
        => await _workspaceRepository.GetByIdWithMembersAsync(id, ct)
            ?? throw new KeyNotFoundException("Workspace not found.");

    private static void EnsureIsMember(Workspace workspace, Guid userId)
    {
        if (!workspace.IsMember(userId))
            throw new KeyNotFoundException("Character not found.");
    }

    private static void EnsureCanCreateForUser(Workspace workspace, Guid requestingUserId, Guid characterUserId)
    {
        if (!workspace.IsMember(characterUserId))
            throw new KeyNotFoundException("Workspace member not found.");

        if (!workspace.IsOwnerOrMaster(requestingUserId))
            throw new UnauthorizedAccessException("Only Owner or Master can create characters.");
    }

    private Task<Workspace?> ResolveWorkspaceAsync(Guid? campaignId, CancellationToken ct)
        => CharacterAuthorizationHelper.ResolveWorkspaceAsync(_campaignRepository, _workspaceRepository, campaignId, ct);

    private static void EnsureCanViewCharacter(Character character, Workspace? workspace, Guid requestingUserId)
    {
        if (workspace is null)
        {
            if (character.UserId != requestingUserId)
                throw new KeyNotFoundException("Character not found.");
            return;
        }

        EnsureIsMember(workspace, requestingUserId);
    }

    private static CharacterResponse ToResponse(Character c) =>
        new(c.Id.ToString(), c.CampaignId?.ToString(), c.UserId.ToString(), c.Name, c.Description,
            c.Race, c.Class, c.Level, c.Status,
            c.PortraitAssetId.HasValue ? $"/api/characters/{c.Id}/portrait" : null,
            c.HpCurrent, c.HpMax, c.MpCurrent, c.MpMax, c.RetrospectiveText,
            c.RecapText, c.RecapGeneratedAt, c.AccentColor, c.PublicShareToken, c.CreatedAt, c.UpdatedAt);
}
