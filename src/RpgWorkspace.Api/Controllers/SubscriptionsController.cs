using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RpgWorkspace.Application.DTOs.Subscription;
using RpgWorkspace.Application.Interfaces;

namespace RpgWorkspace.Api.Controllers;

[Authorize]
public class SubscriptionsController : ApiController
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IHostEnvironment _env;

    public SubscriptionsController(ISubscriptionService subscriptionService, IHostEnvironment env)
    {
        _subscriptionService = subscriptionService;
        _env = env;
    }

    /// <summary>Retorna o status de assinatura do usuário autenticado.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(SubscriptionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _subscriptionService.GetStatusAsync(userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Inicia o checkout de assinatura. Retorna 501 enquanto o gateway de pagamento não está configurado.</summary>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(CheckoutSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> Checkout([FromBody] StartCheckoutRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _subscriptionService.StartCheckoutAsync(userId, request, cancellationToken);
            return Ok(result);
        }
        catch (NotSupportedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new { message = ex.Message });
        }
    }

    /// <summary>Ativa/desativa manualmente a assinatura do usuário. Somente em desenvolvimento.</summary>
    [HttpPost("manual-override")]
    [ProducesResponseType(typeof(SubscriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetManualOverride(
        [FromBody] SetManualOverrideRequest request, CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        var userId = GetCurrentUserId();
        var result = await _subscriptionService.SetManualOverrideAsync(userId, request.Enabled, cancellationToken);
        return Ok(result);
    }

    /// <summary>Webhook do gateway de pagamento. Retorna 501 enquanto o gateway não está configurado.</summary>
    [AllowAnonymous]
    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        Request.Headers.TryGetValue("Stripe-Signature", out var signature);
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);

        try
        {
            await _subscriptionService.HandleWebhookAsync(payload, signature.ToString(), cancellationToken);
            return Ok();
        }
        catch (NotSupportedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new { message = ex.Message });
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
