using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface IAiUsageRepository
{
    Task<AiUsage?> GetByUserAndPeriodAsync(Guid userId, string period, CancellationToken cancellationToken = default);
    Task AddAsync(AiUsage usage, CancellationToken cancellationToken = default);
}
