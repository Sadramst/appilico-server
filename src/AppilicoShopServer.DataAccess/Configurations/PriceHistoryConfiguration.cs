using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for PriceHistory entity.
/// </summary>
public class PriceHistoryConfiguration : IEntityTypeConfiguration<PriceHistory>
{
    /// <summary>Configures the PriceHistory entity.</summary>
    public void Configure(EntityTypeBuilder<PriceHistory> builder)
    {
        builder.HasKey(ph => ph.Id);
        builder.Property(ph => ph.OldPrice).HasColumnType("numeric(18,2)");
        builder.Property(ph => ph.NewPrice).HasColumnType("numeric(18,2)");
        builder.HasQueryFilter(ph => !ph.IsDeleted);
    }
}
