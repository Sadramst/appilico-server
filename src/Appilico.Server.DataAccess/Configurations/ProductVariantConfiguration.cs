using Appilico.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appilico.Server.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for ProductVariant entity.
/// </summary>
public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    /// <summary>Configures the ProductVariant entity.</summary>
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.VariantName).HasMaxLength(200).IsRequired();
        builder.Property(v => v.SKU).HasMaxLength(50).IsRequired();
        builder.Property(v => v.Price).HasColumnType("decimal(18,2)");
        builder.HasQueryFilter(v => !v.IsDeleted);
    }
}
