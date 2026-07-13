using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.Auth;

public sealed record ResetPasswordRequest(
    [Required] string Token,
    [Required, MinLength(6), MaxLength(100)] string NewPassword
);
