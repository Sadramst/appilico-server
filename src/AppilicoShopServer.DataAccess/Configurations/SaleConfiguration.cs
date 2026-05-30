using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for Sale entity.
/// </summary>
public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    /// <summary>Configures the Sale entity.</summary>
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.TotalAmount).HasColumnType("numeric(18,2)");
        builder.Property(s => s.TransactionReference).HasMaxLength(200);
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
