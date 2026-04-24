using Appilico.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appilico.Server.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for Cart entity.
/// </summary>
public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    /// <summary>Configures the Cart entity.</summary>
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.SessionId).HasMaxLength(200);

        builder.HasMany(c => c.Items)
            .WithOne(ci => ci.Cart)
            .HasForeignKey(ci => ci.CartId);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
