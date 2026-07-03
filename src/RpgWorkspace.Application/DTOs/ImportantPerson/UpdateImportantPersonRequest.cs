using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.ImportantPerson;

public sealed record UpdateImportantPersonRequest(
    [Required, MaxLength(100)] string Name,
    [Required] ImportantPersonType Type,
    [MaxLength(1000)] string? FirstImpression,
    [MaxLength(2000)] string? Analysis,
    [Required] EvaluationLevel TrustLevel,
    [Required] EvaluationLevel RiskLevel,
    [Required] EvaluationLevel UtilityLevel,
    [MaxLength(2000)] string? Notes,
    DateTime? LastContactAt
);
