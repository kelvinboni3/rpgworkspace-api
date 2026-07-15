using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class AiUsageRepository : IAiUsageRepository
{
    private readonly AppDbContext _context;

    public AiUsageRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<AiUsage?> GetByUserAndPeriodAsync(Guid userId, string period, CancellationToken cancellationToken = default)
        => _context.AiUsages.FirstOrDefaultAsync(u => u.UserId == userId && u.Period == period, cancellationToken);

    public async Task AddAsync(AiUsage usage, CancellationToken cancellationToken = default)
        => await _context.AiUsages.AddAsync(usage, cancellationToken);
}
