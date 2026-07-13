using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RpgWorkspace.Application.DTOs.Auth;
using RpgWorkspace.Application.Exceptions;
using RpgWorkspace.Application.Interfaces;

namespace RpgWorkspace.Api.Controllers;

[AllowAnonymous]
public class AuthController : ApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Registra um novo usuário.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.RegisterAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Register), response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Autentica um usuário e retorna o JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>Envia um e-mail de redefinição de senha, se o e-mail existir. Sempre retorna 200 (não revela se o e-mail está cadastrado).</summary>
    [EnableRateLimiting("password-reset")]
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _authService.RequestPasswordResetAsync(request, cancellationToken);
        return Ok(new { message = "Se o e-mail existir, enviaremos um link de recuperação." });
    }

    /// <summary>Redefine a senha usando o token recebido por e-mail.</summary>
    [EnableRateLimiting("password-reset")]
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _authService.ResetPasswordAsync(request, cancellationToken);
            return Ok(new { message = "Senha redefinida com sucesso." });
        }
        catch (InvalidPasswordResetTokenException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
