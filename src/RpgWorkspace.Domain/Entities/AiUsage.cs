using RpgWorkspace.Domain.Common;

namespace RpgWorkspace.Domain.Entities;

/// <summary>Contador de chamadas de IA por usuário por mês (chave = UserId + Período "yyyy-MM" UTC).
/// Serve de teto de custo por usuário — reseta sozinho pela virada do período, sem cron.</summary>
public sealed class AiUsage : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Period { get; private set; } = string.Empty; // "yyyy-MM" (UTC)
    public int CallCount { get; private set; }

    // EF Core constructor
    private AiUsage() { }

    public static AiUsage Create(Guid userId, string period)
        => new() { UserId = userId, Period = period, CallCount = 0 };

    public void Increment()
    {
        CallCount++;
        SetUpdatedAt();
    }
}
