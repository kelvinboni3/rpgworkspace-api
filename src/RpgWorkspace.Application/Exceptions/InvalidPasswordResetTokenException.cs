namespace RpgWorkspace.Application.Exceptions;

public sealed class InvalidPasswordResetTokenException : Exception
{
    public InvalidPasswordResetTokenException(string message) : base(message) { }
}
