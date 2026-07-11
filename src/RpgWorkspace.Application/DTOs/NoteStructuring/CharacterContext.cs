namespace RpgWorkspace.Application.DTOs.NoteStructuring;

public sealed record CharacterContext(
    string Name,
    string? Race,
    string? Class,
    int Level,
    string? Description
);
