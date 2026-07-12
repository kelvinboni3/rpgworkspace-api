using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.Subscription;

public sealed record StartCheckoutRequest(
    [Required, MaxLength(50)] string Plan
);
