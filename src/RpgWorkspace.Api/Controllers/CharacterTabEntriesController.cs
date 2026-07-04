using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RpgWorkspace.Application.DTOs.CharacterTabEntry;
using RpgWorkspace.Application.Interfaces;

namespace RpgWorkspace.Api.Controllers;

[Authorize]
public class CharacterTabEntriesController : ApiController
{
    private readonly ICharacterTabEntryService _characterTabEntryService;

    public CharacterTabEntriesController(ICharacterTabEntryService characterTabEntryService)
    {
        _characterTabEntryService = characterTabEntryService;
    }

    /// <summary>Lista os blocos de uma aba customizada.</summary>
    [HttpGet("/api/character-tabs/{characterTabId:guid}/entries")]
    [ProducesResponseType(typeof(IReadOnlyList<CharacterTabEntryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllByTab(Guid characterTabId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _characterTabEntryService.GetAllByTabAsync(characterTabId, userId, cancellationToken);
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

    /// <summary>Retorna um bloco pelo Id.</summary>
    [HttpGet("/api/character-tab-entries/{id:guid}")]
    [ProducesResponseType(typeof(CharacterTabEntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _characterTabEntryService.GetByIdAsync(id, userId, cancellationToken);
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

    /// <summary>Cria um bloco de anotação numa aba customizada.</summary>
    [HttpPost("/api/character-tabs/{characterTabId:guid}/entries")]
    [ProducesResponseType(typeof(CharacterTabEntryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        Guid characterTabId,
        [FromBody] CreateCharacterTabEntryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _characterTabEntryService.CreateAsync(characterTabId, request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
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

    /// <summary>Atualiza um bloco de anotação.</summary>
    [HttpPut("/api/character-tab-entries/{id:guid}")]
    [ProducesResponseType(typeof(CharacterTabEntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCharacterTabEntryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _characterTabEntryService.UpdateAsync(id, request, userId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
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

    /// <summary>Exclui um bloco de anotação.</summary>
    [HttpDelete("/api/character-tab-entries/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _characterTabEntryService.DeleteAsync(id, userId, cancellationToken);
            return NoContent();
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
