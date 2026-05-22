using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Appilico.Server.DataAccess.Repositories;

/// <summary>Newsletter subscriber repository.</summary>
public class NewsletterSubscriberRepository : GenericRepository<NewsletterSubscriber>, INewsletterSubscriberRepository
{
    /// <summary>Initializes the repository.</summary>
    public NewsletterSubscriberRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<NewsletterSubscriber?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(subscriber => subscriber.Email == email);
    }

    /// <inheritdoc/>
    public async Task<NewsletterSubscriber?> GetActiveByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(subscriber => subscriber.Email == email && subscriber.IsActive);
    }
}