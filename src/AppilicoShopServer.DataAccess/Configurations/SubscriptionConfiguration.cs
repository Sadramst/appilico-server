using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>EF Core configuration for the Subscription entity.</summary>
public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId).IsRequired();
        builder.Property(s => s.Tier).IsRequired();
        builder.Property(s => s.Status).IsRequired();
        builder.Property(s => s.StartedAt).IsRequired();
        builder.Property(s => s.StripeCustomerId).HasMaxLength(255);
        builder.Property(s => s.StripeSubscriptionId).HasMaxLength(255);
        builder.Property(s => s.StripePriceId).HasMaxLength(255);

        builder.HasIndex(s => s.UserId).IsUnique();

        builder.HasOne(s => s.User)
            .WithOne(u => u.Subscription)
            .HasForeignKey<Subscription>(s => s.UserId);
    }
}
