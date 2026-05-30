using AppilicoShopServer.Domain.Entities;

namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>Repository for newsletter subscribers.</summary>
public interface INewsletterSubscriberRepository : IGenericRepository<NewsletterSubscriber>
{
    /// <summary>Gets a subscriber by normalized email.</summary>
    Task<NewsletterSubscriber?> GetByEmailAsync(string email);

    /// <summary>Gets an active subscriber by normalized email.</summary>
    Task<NewsletterSubscriber?> GetActiveByEmailAsync(string email);
}