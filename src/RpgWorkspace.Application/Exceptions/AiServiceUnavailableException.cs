namespace RpgWorkspace.Application.Exceptions;

public sealed class AiServiceUnavailableException : Exception
{
    public AiServiceUnavailableException(string message) : base(message) { }

    public AiServiceUnavailableException(string message, Exception innerException)
        : base(message, innerException) { }
}
