using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.ImportantPerson;

public sealed record ImportantPersonResponse(
    string Id,
    string CharacterId,
    string Name,
    ImportantPersonType Type,
    string? FirstImpression,
    string? Analysis,
    EvaluationLevel TrustLevel,
    EvaluationLevel RiskLevel,
    EvaluationLevel UtilityLevel,
    string? Notes,
    DateTime? LastContactAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
