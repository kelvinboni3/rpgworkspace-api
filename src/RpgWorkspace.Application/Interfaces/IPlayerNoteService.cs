using RpgWorkspace.Application.DTOs.PlayerNote;

namespace RpgWorkspace.Application.Interfaces;

public interface IPlayerNoteService
{
    Task<IReadOnlyList<PlayerNoteResponse>> GetAllByCharacterAsync(Guid characterId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<PlayerNoteResponse> GetByIdAsync(Guid playerNoteId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<PlayerNoteResponse> CreateAsync(Guid characterId, CreatePlayerNoteRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<PlayerNoteResponse> UpdateAsync(Guid playerNoteId, UpdatePlayerNoteRequest request, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid playerNoteId, Guid requestingUserId, CancellationToken cancellationToken = default);
}
