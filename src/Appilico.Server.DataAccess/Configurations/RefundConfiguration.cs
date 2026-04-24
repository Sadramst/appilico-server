using Appilico.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appilico.Server.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for Refund entity.
/// </summary>
public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    /// <summary>Configures the Refund entity.</summary>
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Amount).HasColumnType("decimal(18,2)");
        builder.Property(r => r.Reason).HasMaxLength(1000);

        builder.HasOne(r => r.Order)
            .WithMany()
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
