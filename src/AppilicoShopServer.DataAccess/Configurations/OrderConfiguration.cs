using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for Order entity.
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <summary>Configures the Order entity.</summary>
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.OrderNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.Property(o => o.SubTotal).HasColumnType("numeric(18,2)");
        builder.Property(o => o.DiscountAmount).HasColumnType("numeric(18,2)");
        builder.Property(o => o.TaxAmount).HasColumnType("numeric(18,2)");
        builder.Property(o => o.ShippingAmount).HasColumnType("numeric(18,2)");
        builder.Property(o => o.TotalAmount).HasColumnType("numeric(18,2)");
        builder.Property(o => o.VoucherCode).HasMaxLength(50);
        builder.Property(o => o.Notes).HasMaxLength(1000);

        builder.HasOne(o => o.ShippingAddress)
            .WithMany()
            .HasForeignKey(o => o.ShippingAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.BillingAddress)
            .WithMany()
            .HasForeignKey(o => o.BillingAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId);

        builder.HasMany(o => o.StatusHistory)
            .WithOne(sh => sh.Order)
            .HasForeignKey(sh => sh.OrderId);

        builder.HasMany(o => o.Payments)
            .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId);

        builder.HasOne(o => o.Sale)
            .WithOne(s => s.Order)
            .HasForeignKey<Sale>(s => s.OrderId);

        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}
