using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RpgWorkspace.Application.DTOs.Public;
using RpgWorkspace.Application.Interfaces;

namespace RpgWorkspace.Api.Controllers;

/// <summary>
/// Unauthenticated read-only view of a character shared via a public token.
/// Only tabs the owner explicitly flagged as public are ever returned.
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("api/public/characters")]
public sealed class PublicCharactersController : ControllerBase
{
    private readonly IPublicCharacterService _publicCharacterService;

    public PublicCharactersController(IPublicCharacterService publicCharacterService)
    {
        _publicCharacterService = publicCharacterService;
    }

    /// <summary>Retorna a ficha pública (card + abas marcadas como públicas) de um personagem pelo token.</summary>
    [HttpGet("{token}")]
    [ProducesResponseType(typeof(PublicCharacterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByToken(string token, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _publicCharacterService.GetByTokenAsync(token, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Este link de personagem não existe ou foi desativado." });
        }
    }

    /// <summary>Retorna os bytes do retrato do personagem compartilhado.</summary>
    [HttpGet("{token}/portrait")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPortrait(string token, CancellationToken cancellationToken)
    {
        try
        {
            var (content, contentType) = await _publicCharacterService.GetPortraitAsync(token, cancellationToken);
            return File(content, contentType);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>Retorna os bytes da imagem de um bloco pertencente a uma aba pública do personagem compartilhado.</summary>
    [HttpGet("{token}/blocks/{blockId:guid}/image")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBlockImage(string token, Guid blockId, CancellationToken cancellationToken)
    {
        try
        {
            var (content, contentType) = await _publicCharacterService.GetBlockImageAsync(token, blockId, cancellationToken);
            return File(content, contentType);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
