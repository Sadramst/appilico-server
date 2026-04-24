using Appilico.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appilico.Server.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for CartItem entity.
/// </summary>
public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    /// <summary>Configures the CartItem entity.</summary>
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(ci => ci.Id);
        builder.Property(ci => ci.UnitPrice).HasColumnType("decimal(18,2)");

        builder.HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ci => ci.Variant)
            .WithMany()
            .HasForeignKey(ci => ci.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(ci => !ci.IsDeleted);
    }
}
