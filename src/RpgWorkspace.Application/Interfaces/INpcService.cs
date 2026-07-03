using RpgWorkspace.Application.DTOs.Npc;

namespace RpgWorkspace.Application.Interfaces;

public interface INpcService
{
    Task<IReadOnlyList<NpcResponse>> GetAllByCampaignAsync(Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<NpcResponse> GetByIdAsync(Guid npcId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<NpcResponse> CreateAsync(Guid campaignId, CreateNpcRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<NpcResponse> UpdateAsync(Guid npcId, UpdateNpcRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid npcId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
