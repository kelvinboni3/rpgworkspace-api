using RpgWorkspace.Application.DTOs.NoteStructuring;

namespace RpgWorkspace.Application.Interfaces;

public interface INoteStructuringService
{
    Task<StructureNoteResponse> StructureNoteAsync(
        Guid characterId,
        StructureNoteRequest request,
        Guid requestingUserId,
        CancellationToken cancellationToken = default);
}
