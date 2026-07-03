using RpgWorkspace.Application.DTOs.PlayerNote;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class PlayerNoteService : IPlayerNoteService
{
    private readonly IPlayerNoteRepository _playerNoteRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PlayerNoteService(
        IPlayerNoteRepository playerNoteRepository,
        ICharacterRepository characterRepository,
        ICampaignRepository campaignRepository,
        ISessionRepository sessionRepository,
        IWorkspaceRepository workspaceRepository,
        ITagRepository tagRepository,
        IUnitOfWork unitOfWork)
    {
        _playerNoteRepository = playerNoteRepository;
        _characterRepository = characterRepository;
        _campaignRepository = campaignRepository;
        _sessionRepository = sessionRepository;
        _workspaceRepository = workspaceRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<PlayerNoteResponse>> GetAllByCharacterAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanViewNotes(workspace, requestingUserId, character);

        var notes = await _playerNoteRepository.GetAllByCharacterAsync(characterId, cancellationToken);

        var responses = new List<PlayerNoteResponse>();
        foreach (var note in notes)
            responses.Add(await ToResponseAsync(note, cancellationToken));

        return responses;
    }

    public async Task<PlayerNoteResponse> GetByIdAsync(
        Guid playerNoteId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var note = await GetPlayerNoteOrThrowAsync(playerNoteId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(note.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanViewNotes(workspace, requestingUserId, character);

        return await ToResponseAsync(note, cancellationToken);
    }

    public async Task<PlayerNoteResponse> CreateAsync(
        Guid characterId, CreatePlayerNoteRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageNotes(workspace, requestingUserId, character);
        await EnsureSessionBelongsToCampaignAsync(request.SessionId, character.CampaignId, cancellationToken);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, character.CampaignId, request.TagIds, cancellationToken);

        var note = PlayerNote.Create(characterId, request.SessionId, request.Title, request.Content, request.Tags);

        await _playerNoteRepository.AddAsync(note, cancellationToken);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("PlayerNote", note.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(note, cancellationToken);
    }

    public async Task<PlayerNoteResponse> UpdateAsync(
        Guid playerNoteId, UpdatePlayerNoteRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var note = await GetPlayerNoteOrThrowAsync(playerNoteId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(note.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageNotes(workspace, requestingUserId, character);
        await EnsureSessionBelongsToCampaignAsync(request.SessionId, character.CampaignId, cancellationToken);
        if (request.TagIds is not null)
            await TagAssociationHelper.ValidateTagsBelongToCampaignAsync(_tagRepository, character.CampaignId, request.TagIds, cancellationToken);

        note.Update(request.SessionId, request.Title, request.Content, request.Tags);
        if (request.TagIds is not null)
            await _tagRepository.ReplaceTagsAsync("PlayerNote", note.Id, request.TagIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(note, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid playerNoteId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var note = await GetPlayerNoteOrThrowAsync(playerNoteId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(note.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageNotes(workspace, requestingUserId, character);

        _playerNoteRepository.Remove(note);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<PlayerNote> GetPlayerNoteOrThrowAsync(Guid id, CancellationToken ct)
        => await _playerNoteRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Player note not found.");

    private async Task<Character> GetCharacterOrThrowAsync(Guid id, CancellationToken ct)
        => await _characterRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Character not found.");

    private async Task<Workspace> GetWorkspaceForCampaignOrThrowAsync(Guid campaignId, CancellationToken ct)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, ct)
            ?? throw new KeyNotFoundException("Campaign not found.");

        return await _workspaceRepository.GetByIdWithMembersAsync(campaign.WorkspaceId, ct)
            ?? throw new KeyNotFoundException("Workspace not found.");
    }

    private async Task EnsureSessionBelongsToCampaignAsync(Guid? sessionId, Guid campaignId, CancellationToken ct)
    {
        if (sessionId is null)
            return;

        var session = await _sessionRepository.GetByIdAsync(sessionId.Value, ct)
            ?? throw new KeyNotFoundException("Session not found.");

        if (session.CampaignId != campaignId)
            throw new ArgumentException("Session must belong to the same campaign as the character.");
    }

    private static void EnsureCanViewNotes(Workspace workspace, Guid requestingUserId, Character character)
    {
        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Player note not found.");

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can view these notes.");
    }

    private static void EnsureCanManageNotes(Workspace workspace, Guid requestingUserId, Character character)
    {
        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Player note not found.");

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can perform this action.");
    }

    private async Task<PlayerNoteResponse> ToResponseAsync(PlayerNote note, CancellationToken ct)
    {
        var tags = await _tagRepository.GetTagsForEntityAsync("PlayerNote", note.Id, ct);
        return new PlayerNoteResponse(
            note.Id.ToString(),
            note.CharacterId.ToString(),
            note.SessionId?.ToString(),
            note.Title,
            note.Content,
            note.Tags,
            note.CreatedAt,
            note.UpdatedAt,
            TagAssociationHelper.ToResponses(tags));
    }
}
