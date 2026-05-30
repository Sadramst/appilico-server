using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppilicoShopServer.DataAccess.Repositories;

/// <summary>Repository for external webhook idempotency records.</summary>
public class ExternalWebhookEventRepository : GenericRepository<ExternalWebhookEvent>, IExternalWebhookEventRepository
{
    /// <summary>Initializes the repository.</summary>
    public ExternalWebhookEventRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<bool> HasProcessedAsync(string provider, string eventId)
    {
        return await _dbSet.AnyAsync(webhookEvent => webhookEvent.Provider == provider && webhookEvent.EventId == eventId);
    }
}