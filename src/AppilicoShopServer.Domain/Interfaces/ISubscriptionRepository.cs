using AppilicoShopServer.Domain.Entities;

namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>Repository for subscription records and history.</summary>
public interface ISubscriptionRepository : IGenericRepository<Subscription>
{
    /// <summary>Gets a subscription by user ID.</summary>
    Task<Subscription?> GetByUserIdAsync(string userId);

    /// <summary>Gets a subscription by Stripe subscription ID.</summary>
    Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId);

    /// <summary>Adds a subscription history entry.</summary>
    Task AddHistoryAsync(SubscriptionHistory history);
}