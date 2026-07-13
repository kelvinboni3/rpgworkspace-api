using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PasswordResetToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    void RemoveRange(IEnumerable<PasswordResetToken> tokens);
}
