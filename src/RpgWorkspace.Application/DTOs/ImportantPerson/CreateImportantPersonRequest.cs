using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.ImportantPerson;

public sealed record CreateImportantPersonRequest(
    [Required, MaxLength(100)] string Name,
    ImportantPersonType Type = ImportantPersonType.Other,
    [MaxLength(1000)] string? FirstImpression = null,
    [MaxLength(2000)] string? Analysis = null,
    EvaluationLevel TrustLevel = EvaluationLevel.None,
    EvaluationLevel RiskLevel = EvaluationLevel.None,
    EvaluationLevel UtilityLevel = EvaluationLevel.None,
    [MaxLength(2000)] string? Notes = null,
    DateTime? LastContactAt = null
);
