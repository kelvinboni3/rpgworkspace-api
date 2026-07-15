namespace RpgWorkspace.Application.Exceptions;

/// <summary>Usuário atingiu o teto mensal de chamadas de IA. Mapeado para 429 pelos controllers.</summary>
public sealed class AiQuotaExceededException : Exception
{
    public AiQuotaExceededException(string message) : base(message) { }
}
