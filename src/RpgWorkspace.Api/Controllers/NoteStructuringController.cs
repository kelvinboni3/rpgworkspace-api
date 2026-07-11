using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RpgWorkspace.Application.DTOs.NoteStructuring;
using RpgWorkspace.Application.Exceptions;
using RpgWorkspace.Application.Interfaces;

namespace RpgWorkspace.Api.Controllers;

[Authorize]
[EnableRateLimiting("ai-note-structuring")]
public class NoteStructuringController : ApiController
{
    private readonly INoteStructuringService _noteStructuringService;

    public NoteStructuringController(INoteStructuringService noteStructuringService)
    {
        _noteStructuringService = noteStructuringService;
    }

    /// <summary>Usa IA (Claude Haiku 4.5) para propor blocos estruturados a partir de uma anotação livre.</summary>
    [HttpPost("/api/characters/{characterId:guid}/notes/structure")]
    [ProducesResponseType(typeof(StructureNoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Structure(
        Guid characterId,
        [FromBody] StructureNoteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _noteStructuringService.StructureNoteAsync(characterId, request, userId, cancellationToken);
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
        catch (AiServiceUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
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
