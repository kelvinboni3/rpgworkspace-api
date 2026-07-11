using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.NoteStructuring;

public sealed record ExistingBlockContext(
    Guid Id,
    Guid TabId,
    string TabName,
    CharacterTabBlockType Type,
    string? Title,
    string? Content,
    string? PayloadJson
);
