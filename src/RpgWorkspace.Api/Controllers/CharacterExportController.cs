using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RpgWorkspace.Application.Interfaces;

namespace RpgWorkspace.Api.Controllers;

[Authorize]
public class CharacterExportController : ApiController
{
    private readonly ICharacterExportService _characterExportService;

    public CharacterExportController(ICharacterExportService characterExportService)
    {
        _characterExportService = characterExportService;
    }

    /// <summary>Exporta o diário de um personagem em Markdown para download.</summary>
    [HttpGet("/api/characters/{characterId:guid}/export/markdown")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportMarkdown(Guid characterId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _characterExportService.ExportMarkdownAsync(characterId, userId, cancellationToken);
            var bytes = Encoding.UTF8.GetBytes(result.Content);
            return File(bytes, "text/markdown", result.FileName);
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
