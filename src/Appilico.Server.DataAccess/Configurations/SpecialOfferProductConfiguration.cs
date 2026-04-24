using Appilico.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appilico.Server.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for SpecialOfferProduct entity.
/// </summary>
public class SpecialOfferProductConfiguration : IEntityTypeConfiguration<SpecialOfferProduct>
{
    /// <summary>Configures the SpecialOfferProduct entity.</summary>
    public void Configure(EntityTypeBuilder<SpecialOfferProduct> builder)
    {
        builder.HasKey(sop => sop.Id);
        builder.Property(sop => sop.OfferPrice).HasColumnType("decimal(18,2)");

        builder.HasOne(sop => sop.Product)
            .WithMany(p => p.SpecialOfferProducts)
            .HasForeignKey(sop => sop.ProductId);

        builder.HasQueryFilter(sop => !sop.IsDeleted);
    }
}
