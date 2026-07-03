using RpgWorkspace.Application.DTOs.ImportantPerson;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class ImportantPersonService : IImportantPersonService
{
    private readonly IImportantPersonRepository _importantPersonRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ImportantPersonService(
        IImportantPersonRepository importantPersonRepository,
        ICharacterRepository characterRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork)
    {
        _importantPersonRepository = importantPersonRepository;
        _characterRepository = characterRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ImportantPersonResponse>> GetAllByCharacterAsync(
        Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanViewImportantPeople(workspace, requestingUserId, character);

        var importantPeople = await _importantPersonRepository.GetAllByCharacterAsync(characterId, cancellationToken);

        return importantPeople.Select(ToResponse).ToList();
    }

    public async Task<ImportantPersonResponse> GetByIdAsync(
        Guid importantPersonId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var importantPerson = await GetImportantPersonOrThrowAsync(importantPersonId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(importantPerson.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanViewImportantPeople(workspace, requestingUserId, character);

        return ToResponse(importantPerson);
    }

    public async Task<ImportantPersonResponse> CreateAsync(
        Guid characterId, CreateImportantPersonRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var character = await GetCharacterOrThrowAsync(characterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageImportantPeople(workspace, requestingUserId, character);

        var importantPerson = ImportantPerson.Create(
            characterId,
            request.Name,
            request.Type,
            request.FirstImpression,
            request.Analysis,
            request.TrustLevel,
            request.RiskLevel,
            request.UtilityLevel,
            request.Notes,
            request.LastContactAt);

        await _importantPersonRepository.AddAsync(importantPerson, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(importantPerson);
    }

    public async Task<ImportantPersonResponse> UpdateAsync(
        Guid importantPersonId, UpdateImportantPersonRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var importantPerson = await GetImportantPersonOrThrowAsync(importantPersonId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(importantPerson.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageImportantPeople(workspace, requestingUserId, character);

        importantPerson.Update(
            request.Name,
            request.Type,
            request.FirstImpression,
            request.Analysis,
            request.TrustLevel,
            request.RiskLevel,
            request.UtilityLevel,
            request.Notes,
            request.LastContactAt);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(importantPerson);
    }

    public async Task DeleteAsync(
        Guid importantPersonId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var importantPerson = await GetImportantPersonOrThrowAsync(importantPersonId, cancellationToken);
        var character = await GetCharacterOrThrowAsync(importantPerson.CharacterId, cancellationToken);
        var workspace = await GetWorkspaceForCampaignOrThrowAsync(character.CampaignId, cancellationToken);
        EnsureCanManageImportantPeople(workspace, requestingUserId, character);

        _importantPersonRepository.Remove(importantPerson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<ImportantPerson> GetImportantPersonOrThrowAsync(Guid id, CancellationToken ct)
        => await _importantPersonRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Important person not found.");

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

    private static void EnsureCanViewImportantPeople(Workspace workspace, Guid requestingUserId, Character character)
    {
        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Important person not found.");

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can view these important people.");
    }

    private static void EnsureCanManageImportantPeople(Workspace workspace, Guid requestingUserId, Character character)
    {
        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException("Important person not found.");

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException("Only Owner, Master or the character owner can perform this action.");
    }

    private static ImportantPersonResponse ToResponse(ImportantPerson p) =>
        new(
            p.Id.ToString(),
            p.CharacterId.ToString(),
            p.Name,
            p.Type,
            p.FirstImpression,
            p.Analysis,
            p.TrustLevel,
            p.RiskLevel,
            p.UtilityLevel,
            p.Notes,
            p.LastContactAt,
            p.CreatedAt,
            p.UpdatedAt);
}
