using RpgWorkspace.Application.DTOs.NoteStructuring;

namespace RpgWorkspace.Application.Interfaces;

public interface INoteStructuringGateway
{
    Task<NoteStructuringResult> StructureAsync(
        string noteText,
        CharacterContext character,
        IReadOnlyList<(Guid Id, string Name)> existingTabs,
        IReadOnlyList<ExistingBlockContext> existingBlocks,
        string? preferredTabName,
        Guid requestingUserId,
        CancellationToken cancellationToken = default);

    /// <summary>Extracts a pasted character sheet (from another tool/PDF text) into a full set of
    /// structured blocks — broader coverage than <see cref="StructureAsync"/>, which only structures
    /// what's new in a single post-session note.</summary>
    Task<NoteStructuringResult> ImportSheetAsync(
        string sheetText,
        CharacterContext character,
        IReadOnlyList<(Guid Id, string Name)> existingTabs,
        IReadOnlyList<ExistingBlockContext> existingBlocks,
        string? preferredTabName,
        Guid requestingUserId,
        CancellationToken cancellationToken = default);
}
