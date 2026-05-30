using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for Discount entity.
/// </summary>
public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    /// <summary>Configures the Discount entity.</summary>
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(d => d.Code).IsUnique();
        builder.Property(d => d.Name).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(1000);
        builder.Property(d => d.Value).HasColumnType("numeric(18,2)");
        builder.Property(d => d.MinOrderAmount).HasColumnType("numeric(18,2)");
        builder.Property(d => d.MaxDiscountAmount).HasColumnType("numeric(18,2)");
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
