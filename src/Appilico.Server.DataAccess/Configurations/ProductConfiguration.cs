using Appilico.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appilico.Server.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for Product entity.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <summary>Configures the Product entity.</summary>
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(300).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(5000);
        builder.Property(p => p.SKU).HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.SKU).IsUnique();
        builder.Property(p => p.Barcode).HasMaxLength(50);
        builder.Property(p => p.BasePrice).HasColumnType("numeric(18,2)");
        builder.Property(p => p.CostPrice).HasColumnType("numeric(18,2)");
        builder.Property(p => p.Weight).HasColumnType("numeric(10,2)");
        builder.Property(p => p.Dimensions).HasMaxLength(100);
        builder.Property(p => p.AverageRating).HasColumnType("numeric(3,2)");

        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId);

        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId);

        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId);

        builder.HasMany(p => p.PriceHistories)
            .WithOne(ph => ph.Product)
            .HasForeignKey(ph => ph.ProductId);

        builder.HasMany(p => p.Reviews)
            .WithOne(r => r.Product)
            .HasForeignKey(r => r.ProductId);

        builder.HasMany(p => p.Wishlists)
            .WithOne(w => w.Product)
            .HasForeignKey(w => w.ProductId);

        builder.HasMany(p => p.InventoryTransactions)
            .WithOne(it => it.Product)
            .HasForeignKey(it => it.ProductId);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
