using RpgWorkspace.Domain.Enums;
using RpgWorkspace.Application.DTOs.Tag;

namespace RpgWorkspace.Application.DTOs.Operation;

public sealed record OperationResponse(
    string Id,
    string CharacterId,
    string Name,
    string? Objective,
    string? Plan,
    string? RequiredResources,
    string? Risks,
    OperationStatus Status,
    string? Result,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<TagResponse> Tags
);
