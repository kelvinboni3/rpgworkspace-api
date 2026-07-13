using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AppDbContext _context;

    public PasswordResetTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => _context.PasswordResetTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public async Task<IReadOnlyList<PasswordResetToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.PasswordResetTokens
            .Where(t => t.UserId == userId && t.UsedAt == null)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
        => await _context.PasswordResetTokens.AddAsync(token, cancellationToken);

    public void RemoveRange(IEnumerable<PasswordResetToken> tokens)
        => _context.PasswordResetTokens.RemoveRange(tokens);
}
