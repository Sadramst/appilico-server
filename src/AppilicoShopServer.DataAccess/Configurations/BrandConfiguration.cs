using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for Brand entity.
/// </summary>
public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    /// <summary>Configures the Brand entity.</summary>
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(b => b.Name).IsUnique();
        builder.Property(b => b.Description).HasMaxLength(1000);
        builder.Property(b => b.LogoUrl).HasMaxLength(500);
        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}
