using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.Auth;

public sealed record LoginRequest(
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required] string Password
);
