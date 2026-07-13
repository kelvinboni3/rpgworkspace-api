using RpgWorkspace.Application.DTOs.Subscription;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Tests;

internal sealed class FakeSubscriptionRepository : ISubscriptionRepository
{
    public List<Subscription> Items { get; } = [];

    public Task<Subscription?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(s => s.UserId == userId));

    public Task<Subscription?> GetByGatewayCustomerIdAsync(string gatewayCustomerId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(s => s.GatewayCustomerId == gatewayCustomerId));

    public Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        Items.Add(subscription);
        return Task.CompletedTask;
    }
}

internal sealed class FakeUserRepository : IUserRepository
{
    public List<User> Items { get; } = [];

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(u => u.Email == email.Trim().ToLowerInvariant()));

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(u => u.Email == email.Trim().ToLowerInvariant()));

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        Items.Add(user);
        return Task.CompletedTask;
    }
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}

internal sealed class FakeTokenGenerator : ITokenGenerator
{
    public string GenerateToken(string userId, string email, IEnumerable<string> roles) => $"token-{userId}";
}

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"hash::{password}";

    public bool Verify(string password, string hash) => hash == $"hash::{password}";
}

internal sealed class FakeEmailGateway : IEmailGateway
{
    public List<(string Email, string Name, string Token)> Sent { get; } = [];

    public Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken, CancellationToken cancellationToken = default)
    {
        Sent.Add((toEmail, toName, resetToken));
        return Task.CompletedTask;
    }
}

internal sealed class FakePasswordResetTokenRepository : IPasswordResetTokenRepository
{
    public List<PasswordResetToken> Items { get; } = [];

    public Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(t => t.TokenHash == tokenHash));

    public Task<IReadOnlyList<PasswordResetToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PasswordResetToken>>(Items.Where(t => t.UserId == userId).ToList());

    public Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        Items.Add(token);
        return Task.CompletedTask;
    }

    public void RemoveRange(IEnumerable<PasswordResetToken> tokens)
    {
        foreach (var token in tokens.ToList())
            Items.Remove(token);
    }
}

/// <summary>Gateway fake: checkout returns a fixed URL; webhook parsing returns whatever the test scripted.</summary>
internal sealed class FakeSubscriptionGateway : ISubscriptionGateway
{
    public GatewayWebhookEvent? NextWebhookEvent { get; set; }

    public Task<string> CreateCheckoutSessionAsync(Guid userId, string plan, CancellationToken cancellationToken = default) =>
        Task.FromResult("https://checkout.example/session");

    public Task<GatewayWebhookEvent?> ParseWebhookEventAsync(string payload, string signature, CancellationToken cancellationToken = default) =>
        Task.FromResult(NextWebhookEvent);
}
