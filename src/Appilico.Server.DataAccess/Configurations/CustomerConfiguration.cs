using Appilico.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appilico.Server.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for Customer entity.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    /// <summary>Configures the Customer entity.</summary>
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CustomerCode).HasMaxLength(30).IsRequired();
        builder.HasIndex(c => c.CustomerCode).IsUnique();
        builder.Property(c => c.TotalPurchases).HasColumnType("numeric(18,2)");

        builder.HasMany(c => c.Addresses)
            .WithOne(a => a.Customer)
            .HasForeignKey(a => a.CustomerId);

        builder.HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId);

        builder.HasMany(c => c.Reviews)
            .WithOne(r => r.Customer)
            .HasForeignKey(r => r.CustomerId);

        builder.HasMany(c => c.Wishlists)
            .WithOne(w => w.Customer)
            .HasForeignKey(w => w.CustomerId);

        builder.HasMany(c => c.Carts)
            .WithOne(ct => ct.Customer)
            .HasForeignKey(ct => ct.CustomerId);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
