using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>EF Core configuration for the NewsletterSubscriber entity.</summary>
public class NewsletterSubscriberConfiguration : IEntityTypeConfiguration<NewsletterSubscriber>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<NewsletterSubscriber> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Email).HasMaxLength(255).IsRequired();
        builder.Property(n => n.Source).HasMaxLength(100);

        builder.HasIndex(n => n.Email).IsUnique();
    }
}
