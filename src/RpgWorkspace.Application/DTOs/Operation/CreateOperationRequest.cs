using System.ComponentModel.DataAnnotations;
using RpgWorkspace.Domain.Enums;

namespace RpgWorkspace.Application.DTOs.Operation;

public sealed record CreateOperationRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Objective,
    [MaxLength(3000)] string? Plan,
    [MaxLength(2000)] string? RequiredResources,
    [MaxLength(2000)] string? Risks,
    OperationStatus Status = OperationStatus.Planned,
    [MaxLength(2000)] string? Result = null,
    Guid[]? TagIds = null
);
