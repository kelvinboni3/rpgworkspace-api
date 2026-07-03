using RpgWorkspace.Domain.Enums;
using RpgWorkspace.Application.DTOs.Tag;

namespace RpgWorkspace.Application.DTOs.Theory;

public sealed record TheoryResponse(
    string Id,
    string CharacterId,
    string Title,
    string? Description,
    string? Evidence,
    int Confidence,
    TheoryStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<TagResponse> Tags
);
