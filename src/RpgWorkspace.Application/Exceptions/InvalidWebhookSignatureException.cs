namespace RpgWorkspace.Application.Exceptions;

/// <summary>Webhook payload failed signature verification — mapped to 400 by the controller.</summary>
public sealed class InvalidWebhookSignatureException : Exception
{
    public InvalidWebhookSignatureException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
