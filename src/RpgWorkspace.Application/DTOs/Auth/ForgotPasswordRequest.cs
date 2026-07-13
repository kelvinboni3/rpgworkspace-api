using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.Auth;

public sealed record ForgotPasswordRequest(
    [Required, EmailAddress, MaxLength(200)] string Email
);
