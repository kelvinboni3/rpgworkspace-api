using Microsoft.EntityFrameworkCore;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;
using RpgWorkspace.Infrastructure.Persistence;

namespace RpgWorkspace.Infrastructure.Repositories;

public sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly AppDbContext _context;

    public SubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Subscription?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => _context.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

    public Task<Subscription?> GetByGatewayCustomerIdAsync(string gatewayCustomerId, CancellationToken cancellationToken = default)
        => _context.Subscriptions.FirstOrDefaultAsync(s => s.GatewayCustomerId == gatewayCustomerId, cancellationToken);

    public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
        => await _context.Subscriptions.AddAsync(subscription, cancellationToken);
}
