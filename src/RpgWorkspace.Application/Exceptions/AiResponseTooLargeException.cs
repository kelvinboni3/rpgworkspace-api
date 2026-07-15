namespace RpgWorkspace.Application.Exceptions;

/// <summary>A IA estourou o teto de tokens de saída (stop_reason max_tokens): a resposta foi
/// cortada no meio e nunca vai parsear, não importa quantas vezes se tente. Diferente de
/// <see cref="AiServiceUnavailableException"/> (503, transitório), isto é culpa do tamanho do
/// pedido — mapeado para 400 com mensagem orientando o usuário a dividir o texto.</summary>
public sealed class AiResponseTooLargeException : Exception
{
    public AiResponseTooLargeException(string message) : base(message) { }
}
