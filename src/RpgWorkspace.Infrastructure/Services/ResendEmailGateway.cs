using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Infrastructure.Configuration;

namespace RpgWorkspace.Infrastructure.Services;

public sealed class ResendEmailGateway : IEmailGateway
{
    private readonly HttpClient _httpClient = new() { BaseAddress = new Uri("https://api.resend.com/") };
    private readonly ResendSettings _settings;
    private readonly ILogger<ResendEmailGateway> _logger;

    public ResendEmailGateway(IOptions<ResendSettings> settings, ILogger<ResendEmailGateway> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string toName,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        var resetUrl = $"{_settings.FrontendBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(resetToken)}";

        var payload = new
        {
            from = $"{_settings.FromName} <{_settings.FromEmail}>",
            to = new[] { toEmail },
            subject = "Redefinir sua senha — RPG Workspace",
            html = BuildHtmlBody(toName, resetUrl),
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("emails", payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Resend returned {StatusCode} sending password reset email: {Body}",
                    (int)response.StatusCode, body);
                throw new InvalidOperationException("Failed to send password reset email.");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach Resend API for password reset email.");
            throw new InvalidOperationException("Failed to send password reset email.", ex);
        }
    }

    private static string BuildHtmlBody(string toName, string resetUrl)
    {
        var safeName = WebUtility.HtmlEncode(toName);
        var safeUrl = WebUtility.HtmlEncode(resetUrl);

        return $"""
            <p>Olá, {safeName}.</p>
            <p>Recebemos um pedido para redefinir a senha da sua conta no RPG Workspace. Se foi você, clique no link abaixo (válido por 1 hora):</p>
            <p><a href="{safeUrl}">Redefinir minha senha</a></p>
            <p>Se você não pediu isso, pode ignorar este e-mail com segurança.</p>
            """;
    }
}
