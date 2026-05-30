using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for Voucher entity.
/// </summary>
public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    /// <summary>Configures the Voucher entity.</summary>
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(v => v.Code).IsUnique();
        builder.Property(v => v.Description).HasMaxLength(1000);
        builder.Property(v => v.Value).HasColumnType("numeric(18,2)");
        builder.Property(v => v.MinOrderAmount).HasColumnType("numeric(18,2)");

        builder.HasMany(v => v.Redemptions)
            .WithOne(r => r.Voucher)
            .HasForeignKey(r => r.VoucherId);

        builder.HasQueryFilter(v => !v.IsDeleted);
    }
}
