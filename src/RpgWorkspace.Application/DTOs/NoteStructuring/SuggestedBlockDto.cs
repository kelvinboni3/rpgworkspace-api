using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.NoteStructuring;

public sealed record SuggestedBlockDto(
    Guid? TargetBlockId,
    string? TargetBlockLabel,
    Guid? TargetTabId,
    string? SuggestedNewTabName,
    CharacterTabBlockType Type,
    string? Title,
    string? Content,
    string? PayloadJson
);
