using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RpgWorkspace.Api.Controllers;

[AllowAnonymous]
public class HealthController : ApiController
{
    [HttpGet("/health")]
    public IActionResult Get() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
}
