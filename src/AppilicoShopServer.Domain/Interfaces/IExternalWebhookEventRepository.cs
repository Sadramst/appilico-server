using AppilicoShopServer.Domain.Entities;

namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>Repository for external webhook idempotency records.</summary>
public interface IExternalWebhookEventRepository : IGenericRepository<ExternalWebhookEvent>
{
    /// <summary>Checks whether a provider event has already been processed.</summary>
    Task<bool> HasProcessedAsync(string provider, string eventId);
}