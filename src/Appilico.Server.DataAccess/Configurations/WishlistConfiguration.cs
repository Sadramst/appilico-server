using Appilico.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appilico.Server.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for Wishlist entity.
/// </summary>
public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    /// <summary>Configures the Wishlist entity.</summary>
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.HasKey(w => w.Id);
        builder.HasIndex(w => new { w.CustomerId, w.ProductId }).IsUnique();
        builder.HasQueryFilter(w => !w.IsDeleted);
    }
}
