using RpgWorkspace.Application.DTOs.NoteStructuring;

namespace RpgWorkspace.Application.Interfaces;

/// <summary>Generates free-form narrative text (not structured block suggestions) from a
/// character's existing journal content — used for the pre-session recap and the
/// end-of-campaign retrospective. Separate from <see cref="INoteStructuringGateway"/> because
/// the response shape is plain text, not the suggestions schema.</summary>
public interface ICharacterNarrativeGateway
{
    /// <summary>Short "previously on..." recap covering recent events and open narrative threads.</summary>
    Task<string> GenerateRecapAsync(
        CharacterContext character,
        IReadOnlyList<(Guid Id, string Name)> existingTabs,
        IReadOnlyList<ExistingBlockContext> existingBlocks,
        CancellationToken cancellationToken = default);

    /// <summary>Closing narrative for a character whose campaign has ended.</summary>
    Task<string> GenerateRetrospectiveAsync(
        CharacterContext character,
        IReadOnlyList<(Guid Id, string Name)> existingTabs,
        IReadOnlyList<ExistingBlockContext> existingBlocks,
        CancellationToken cancellationToken = default);
}
