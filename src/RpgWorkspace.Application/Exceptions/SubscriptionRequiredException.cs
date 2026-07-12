namespace RpgWorkspace.Application.Exceptions;

public sealed class SubscriptionRequiredException : Exception
{
    public SubscriptionRequiredException(string message) : base(message) { }
}
