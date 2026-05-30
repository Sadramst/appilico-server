using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>EF configuration for processed external webhook events.</summary>
public class ExternalWebhookEventConfiguration : IEntityTypeConfiguration<ExternalWebhookEvent>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ExternalWebhookEvent> builder)
    {
        builder.HasKey(webhookEvent => webhookEvent.Id);

        builder.Property(webhookEvent => webhookEvent.Provider)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(webhookEvent => webhookEvent.EventId)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(webhookEvent => webhookEvent.EventType)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(webhookEvent => webhookEvent.PayloadHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(webhookEvent => new { webhookEvent.Provider, webhookEvent.EventId })
            .IsUnique();

        builder.HasQueryFilter(webhookEvent => !webhookEvent.IsDeleted);
    }
}