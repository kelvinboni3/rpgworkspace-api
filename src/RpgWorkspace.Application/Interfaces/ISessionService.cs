using RpgWorkspace.Application.DTOs.Session;

namespace RpgWorkspace.Application.Interfaces;

public interface ISessionService
{
    Task<IReadOnlyList<SessionResponse>> GetAllByCampaignAsync(Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<SessionResponse> GetByIdAsync(Guid sessionId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<SessionResponse> CreateAsync(Guid campaignId, CreateSessionRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<SessionResponse> UpdateAsync(Guid sessionId, UpdateSessionRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid sessionId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
