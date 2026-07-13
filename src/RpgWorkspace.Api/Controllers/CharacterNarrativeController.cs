using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RpgWorkspace.Application.DTOs.CharacterNarrative;
using RpgWorkspace.Application.Exceptions;
using RpgWorkspace.Application.Interfaces;

namespace RpgWorkspace.Api.Controllers;

[Authorize]
[EnableRateLimiting("ai-note-structuring")]
public class CharacterNarrativeController : ApiController
{
    private readonly ICharacterNarrativeService _characterNarrativeService;

    public CharacterNarrativeController(ICharacterNarrativeService characterNarrativeService)
    {
        _characterNarrativeService = characterNarrativeService;
    }

    /// <summary>Usa IA (Claude Haiku 4.5) para gerar um recap curto ("anteriormente, nessa história...") do estado atual do personagem.</summary>
    [HttpPost("/api/characters/{characterId:guid}/recap")]
    [ProducesResponseType(typeof(CharacterRecapResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Recap(Guid characterId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _characterNarrativeService.GenerateRecapAsync(characterId, userId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (SubscriptionRequiredException ex)
        {
            return StatusCode(StatusCodes.Status402PaymentRequired, new { message = ex.Message });
        }
        catch (AiServiceUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }

    /// <summary>Usa IA (Claude Haiku 4.5) para gerar e salvar a retrospectiva final da jornada do personagem.</summary>
    [HttpPost("/api/characters/{characterId:guid}/retrospective")]
    [ProducesResponseType(typeof(CharacterRetrospectiveResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Retrospective(Guid characterId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _characterNarrativeService.GenerateRetrospectiveAsync(characterId, userId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (SubscriptionRequiredException ex)
        {
            return StatusCode(StatusCodes.Status402PaymentRequired, new { message = ex.Message });
        }
        catch (AiServiceUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }

    /// <summary>Resurfaces a random old journal block ("há X dias você escreveu isto..."). Pure DB read, no AI call — not subject to the AI rate limit.</summary>
    [HttpGet("/api/characters/{characterId:guid}/memory")]
    [DisableRateLimiting]
    [ProducesResponseType(typeof(CharacterMemoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Memory(Guid characterId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _characterNarrativeService.GetMemoryAsync(characterId, userId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub")
               ?? throw new InvalidOperationException("User ID claim not found.");

        return Guid.Parse(sub);
    }
}
