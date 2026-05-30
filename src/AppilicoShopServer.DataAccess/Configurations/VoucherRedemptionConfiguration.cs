using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for VoucherRedemption entity.
/// </summary>
public class VoucherRedemptionConfiguration : IEntityTypeConfiguration<VoucherRedemption>
{
    /// <summary>Configures the VoucherRedemption entity.</summary>
    public void Configure(EntityTypeBuilder<VoucherRedemption> builder)
    {
        builder.HasKey(vr => vr.Id);
        builder.Property(vr => vr.AmountDiscounted).HasColumnType("numeric(18,2)");

        builder.HasOne(vr => vr.Customer)
            .WithMany(c => c.VoucherRedemptions)
            .HasForeignKey(vr => vr.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(vr => vr.Order)
            .WithMany(o => o.VoucherRedemptions)
            .HasForeignKey(vr => vr.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(vr => !vr.IsDeleted);
    }
}
