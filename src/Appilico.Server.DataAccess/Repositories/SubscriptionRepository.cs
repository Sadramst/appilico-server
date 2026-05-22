using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Appilico.Server.DataAccess.Repositories;

/// <summary>Subscription repository.</summary>
public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
{
    /// <summary>Initializes the repository.</summary>
    public SubscriptionRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<Subscription?> GetByUserIdAsync(string userId)
    {
        return await _dbSet.FirstOrDefaultAsync(subscription => subscription.UserId == userId);
    }

    /// <inheritdoc/>
    public async Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId)
    {
        return await _dbSet.FirstOrDefaultAsync(subscription => subscription.StripeSubscriptionId == stripeSubscriptionId);
    }

    /// <inheritdoc/>
    public async Task AddHistoryAsync(SubscriptionHistory history)
    {
        await _context.SubscriptionHistories.AddAsync(history);
    }
}