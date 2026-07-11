namespace RpgWorkspace.Application.DTOs.NoteStructuring;

public sealed record StructureNoteResponse(
    IReadOnlyList<SuggestedBlockDto> Suggestions
);
