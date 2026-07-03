using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.Auth;

public sealed record RegisterRequest(
    [Required, MaxLength(100)] string Name,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password
);
