using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for OrderStatusHistory entity.
/// </summary>
public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    /// <summary>Configures the OrderStatusHistory entity.</summary>
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.HasKey(osh => osh.Id);
        builder.Property(osh => osh.Notes).HasMaxLength(500);
        builder.HasQueryFilter(osh => !osh.IsDeleted);
    }
}
