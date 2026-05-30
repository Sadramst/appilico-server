using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for CustomerAddress entity.
/// </summary>
public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    /// <summary>Configures the CustomerAddress entity.</summary>
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Title).HasMaxLength(50).IsRequired();
        builder.Property(a => a.AddressLine1).HasMaxLength(200).IsRequired();
        builder.Property(a => a.AddressLine2).HasMaxLength(200);
        builder.Property(a => a.City).HasMaxLength(100).IsRequired();
        builder.Property(a => a.State).HasMaxLength(100);
        builder.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
        builder.Property(a => a.Country).HasMaxLength(100).IsRequired();
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
