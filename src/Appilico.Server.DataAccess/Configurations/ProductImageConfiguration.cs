using Appilico.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appilico.Server.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for ProductImage entity.
/// </summary>
public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    /// <summary>Configures the ProductImage entity.</summary>
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasKey(pi => pi.Id);
        builder.Property(pi => pi.ImageUrl).HasMaxLength(500).IsRequired();
        builder.Property(pi => pi.AltText).HasMaxLength(200);
        builder.HasQueryFilter(pi => !pi.IsDeleted);
    }
}
