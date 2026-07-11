using RpgWorkspace.Application.DTOs.NoteStructuring;

namespace RpgWorkspace.Application.Interfaces;

public interface INoteStructuringGateway
{
    Task<IReadOnlyList<SuggestedBlockDto>> StructureAsync(
        string noteText,
        CharacterContext character,
        IReadOnlyList<(Guid Id, string Name)> existingTabs,
        IReadOnlyList<ExistingBlockContext> existingBlocks,
        CancellationToken cancellationToken = default);
}
