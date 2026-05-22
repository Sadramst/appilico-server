using Appilico.Server.Domain.Common;

namespace Appilico.Server.Domain.Entities;

/// <summary>Tracks processed external webhook events for idempotency.</summary>
public class ExternalWebhookEvent : BaseAuditableEntity
{
    /// <summary>External provider name.</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>Provider event ID.</summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>Provider event type.</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Hash of the processed raw payload.</summary>
    public string PayloadHash { get; set; } = string.Empty;

    /// <summary>UTC timestamp when processing completed.</summary>
    public DateTime ProcessedAt { get; set; }
}