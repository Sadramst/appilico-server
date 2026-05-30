using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for InventoryTransaction entity.
/// </summary>
public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    /// <summary>Configures the InventoryTransaction entity.</summary>
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.HasKey(it => it.Id);
        builder.Property(it => it.Reference).HasMaxLength(200);
        builder.Property(it => it.Notes).HasMaxLength(500);

        builder.HasOne(it => it.Variant)
            .WithMany()
            .HasForeignKey(it => it.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(it => !it.IsDeleted);
    }
}
