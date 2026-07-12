using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

/// <summary>
/// Shared Campaign→Workspace resolution and owner-or-GM authorization for character-scoped
/// content (tabs, blocks, dashboard, etc). A solo character (CampaignId null) has no Workspace,
/// so Workspace is null and the only possible viewer/manager is the character's own owner.
/// </summary>
internal static class CharacterAuthorizationHelper
{
    public static async Task<Workspace?> ResolveWorkspaceAsync(
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        Guid? campaignId,
        CancellationToken cancellationToken)
    {
        if (campaignId is null)
            return null;

        var campaign = await campaignRepository.GetByIdAsync(campaignId.Value, cancellationToken)
            ?? throw new KeyNotFoundException("Campaign not found.");

        return await workspaceRepository.GetByIdWithMembersAsync(campaign.WorkspaceId, cancellationToken)
            ?? throw new KeyNotFoundException("Workspace not found.");
    }

    public static void EnsureCanView(Character character, Workspace? workspace, Guid requestingUserId, string notFoundMessage)
        => Ensure(character, workspace, requestingUserId, notFoundMessage,
            "Only Owner, Master or the character owner can view this resource.");

    public static void EnsureCanManage(Character character, Workspace? workspace, Guid requestingUserId, string notFoundMessage)
        => Ensure(character, workspace, requestingUserId, notFoundMessage,
            "Only Owner, Master or the character owner can perform this action.");

    private static void Ensure(
        Character character, Workspace? workspace, Guid requestingUserId, string notFoundMessage, string forbiddenMessage)
    {
        if (workspace is null)
        {
            if (character.UserId != requestingUserId)
                throw new KeyNotFoundException(notFoundMessage);
            return;
        }

        if (!workspace.IsMember(requestingUserId))
            throw new KeyNotFoundException(notFoundMessage);

        if (requestingUserId == character.UserId || workspace.IsOwnerOrMaster(requestingUserId))
            return;

        throw new UnauthorizedAccessException(forbiddenMessage);
    }
}
