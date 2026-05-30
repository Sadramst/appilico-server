using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for SpecialOffer entity.
/// </summary>
public class SpecialOfferConfiguration : IEntityTypeConfiguration<SpecialOffer>
{
    /// <summary>Configures the SpecialOffer entity.</summary>
    public void Configure(EntityTypeBuilder<SpecialOffer> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Name).HasMaxLength(200).IsRequired();
        builder.Property(o => o.Description).HasMaxLength(1000);
        builder.Property(o => o.BannerImageUrl).HasMaxLength(500);

        builder.HasMany(o => o.SpecialOfferProducts)
            .WithOne(sop => sop.SpecialOffer)
            .HasForeignKey(sop => sop.SpecialOfferId);

        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}
