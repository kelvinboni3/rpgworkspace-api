using RpgWorkspace.Application.DTOs.AiUsage;

namespace RpgWorkspace.Application.Interfaces;

public interface IAiUsageService
{
    /// <summary>Lança <see cref="Exceptions.AiQuotaExceededException"/> se o usuário já atingiu o limite mensal.</summary>
    Task EnsureWithinQuotaAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Incrementa o contador do mês. Chamar só depois de uma chamada de IA bem-sucedida.</summary>
    Task TrackAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<AiUsageStatus> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default);
}
