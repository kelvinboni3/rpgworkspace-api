namespace RpgWorkspace.Application.DTOs.NoteStructuring;

public sealed record NoteStructuringResult(
    string? Summary,
    IReadOnlyList<SuggestedBlockDto> Suggestions
);
