using Appilico.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appilico.Server.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for OrderItem entity.
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    /// <summary>Configures the OrderItem entity.</summary>
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);
        builder.Property(oi => oi.ProductName).HasMaxLength(300).IsRequired();
        builder.Property(oi => oi.UnitPrice).HasColumnType("numeric(18,2)");
        builder.Property(oi => oi.TotalPrice).HasColumnType("numeric(18,2)");
        builder.Property(oi => oi.Discount).HasColumnType("numeric(18,2)");

        builder.HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(oi => oi.Variant)
            .WithMany()
            .HasForeignKey(oi => oi.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(oi => !oi.IsDeleted);
    }
}
