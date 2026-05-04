using Appilico.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appilico.Server.DataAccess.Configurations;

/// <summary>EF Core configuration for the SubscriptionHistory entity.</summary>
public class SubscriptionHistoryConfiguration : IEntityTypeConfiguration<SubscriptionHistory>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SubscriptionHistory> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.UserId).IsRequired();
        builder.Property(h => h.Reason).HasMaxLength(500);
        builder.Property(h => h.ChangedBy).HasMaxLength(255);

        builder.HasOne(h => h.Subscription)
            .WithMany(s => s.History)
            .HasForeignKey(h => h.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
