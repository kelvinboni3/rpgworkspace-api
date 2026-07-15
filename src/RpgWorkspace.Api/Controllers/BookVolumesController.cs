using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RpgWorkspace.Application.DTOs.BookVolume;
using RpgWorkspace.Application.Interfaces;

namespace RpgWorkspace.Api.Controllers;

[Authorize]
public class BookVolumesController : ApiController
{
    private readonly IBookVolumeService _bookVolumeService;

    public BookVolumesController(IBookVolumeService bookVolumeService)
    {
        _bookVolumeService = bookVolumeService;
    }

    /// <summary>Lista os volumes (PDFs) de um bloco Livro.</summary>
    [HttpGet("/api/character-tab-blocks/{characterTabBlockId:guid}/book-volumes")]
    [ProducesResponseType(typeof(IReadOnlyList<BookVolumeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllByBlock(Guid characterTabBlockId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _bookVolumeService.GetAllByBlockAsync(characterTabBlockId, userId, cancellationToken);
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

    /// <summary>Sobe um novo PDF como próximo volume do livro.</summary>
    [HttpPost("/api/character-tab-blocks/{characterTabBlockId:guid}/book-volumes")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(21 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 21 * 1024 * 1024)]
    [ProducesResponseType(typeof(BookVolumeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upload(
        Guid characterTabBlockId, IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            await using var stream = file.OpenReadStream();
            var result = await _bookVolumeService.UploadAsync(
                characterTabBlockId, file.FileName, file.ContentType, file.Length, stream, userId, cancellationToken);
            return CreatedAtAction(nameof(GetAllByBlock), new { characterTabBlockId }, result);
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

    /// <summary>Reordena os volumes de um livro.</summary>
    [HttpPut("/api/character-tab-blocks/{characterTabBlockId:guid}/book-volumes/reorder")]
    [ProducesResponseType(typeof(IReadOnlyList<BookVolumeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reorder(
        Guid characterTabBlockId, [FromBody] ReorderBookVolumesRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _bookVolumeService.ReorderAsync(characterTabBlockId, request, userId, cancellationToken);
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

    /// <summary>Exclui um volume do livro.</summary>
    [HttpDelete("/api/book-volumes/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _bookVolumeService.DeleteAsync(id, userId, cancellationToken);
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

    /// <summary>Retorna os bytes do PDF de um volume.</summary>
    [HttpGet("/api/book-volumes/{id:guid}/content")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContent(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var (content, contentType) = await _bookVolumeService.GetContentAsync(id, userId, cancellationToken);
            return File(content, contentType);
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
