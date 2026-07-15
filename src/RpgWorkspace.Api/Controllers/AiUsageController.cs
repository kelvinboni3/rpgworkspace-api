using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RpgWorkspace.Application.DTOs.AiUsage;
using RpgWorkspace.Application.Interfaces;

namespace RpgWorkspace.Api.Controllers;

[Authorize]
public class AiUsageController : ApiController
{
    private readonly IAiUsageService _aiUsageService;

    public AiUsageController(IAiUsageService aiUsageService)
    {
        _aiUsageService = aiUsageService;
    }

    /// <summary>Quanto de IA o usuário já usou no mês e quanto resta (para o app mostrar transparência).</summary>
    [HttpGet("/api/ai/usage")]
    [ProducesResponseType(typeof(AiUsageStatus), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsage(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var status = await _aiUsageService.GetStatusAsync(userId, cancellationToken);
        return Ok(status);
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub")
               ?? throw new InvalidOperationException("User ID claim not found.");

        return Guid.Parse(sub);
    }
}
