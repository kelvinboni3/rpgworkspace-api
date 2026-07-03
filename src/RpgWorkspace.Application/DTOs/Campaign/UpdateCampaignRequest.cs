using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.Campaign;

public sealed record UpdateCampaignRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [MaxLength(100)] string? SystemName
);
