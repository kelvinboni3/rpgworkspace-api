namespace RpgWorkspace.Application.Interfaces;

public interface IEmailGateway
{
    /// <summary>Builds the reset link from the raw token and sends it — URL construction is an
    /// infrastructure concern (frontend base URL is Infrastructure-layer config), not Application's.</summary>
    Task SendPasswordResetEmailAsync(
        string toEmail,
        string toName,
        string resetToken,
        CancellationToken cancellationToken = default);
}
