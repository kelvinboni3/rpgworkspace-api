using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using RpgWorkspace.Api.Extensions;
using RpgWorkspace.Api.Middlewares;
using RpgWorkspace.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// The placeholder secret is committed in appsettings.json — anyone could forge tokens with it.
if (!builder.Environment.IsDevelopment()
    && (builder.Configuration["JwtSettings:Secret"] ?? string.Empty).StartsWith("CHANGE-THIS"))
{
    throw new InvalidOperationException(
        "JwtSettings:Secret must be set via environment variable (JwtSettings__Secret) outside Development.");
}

// ── Services ────────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// TLS terminates at the host's reverse proxy (Railway/Render) — without these headers the app
// sees plain http and the password-reset rate limiter sees the proxy's IP for every user.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddFrontendCors(builder.Configuration);
builder.Services.AddSwagger();
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

var aiRateLimitPerHour = builder.Configuration.GetValue("Anthropic:RateLimitPerHour", 20);
var passwordResetRateLimitPerHour = builder.Configuration.GetValue("Resend:RateLimitPerHour", 5);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        var policyName = context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName;
        var message = policyName == "password-reset"
            ? "Muitas tentativas. Tente novamente mais tarde."
            : "Limite de uso da IA atingido. Tente novamente mais tarde.";

        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            JsonSerializer.Serialize(new { message }),
            cancellationToken);
    };

    options.AddPolicy("ai-note-structuring", httpContext =>
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.User.FindFirstValue("sub")
            ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = aiRateLimitPerHour,
            Window = TimeSpan.FromHours(1),
            QueueLimit = 0,
        });
    });

    // Anonymous endpoint (no user identity yet) — partition by IP instead of user id.
    options.AddPolicy("password-reset", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = passwordResetRateLimitPerHour,
            Window = TimeSpan.FromHours(1),
            QueueLimit = 0,
        });
    });
});

// ── Pipeline ─────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseForwardedHeaders();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();
}

app.UseHttpsRedirection();

app.UseCors(CorsExtensions.PolicyName);

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthCheckEndpoints();

app.Run();
