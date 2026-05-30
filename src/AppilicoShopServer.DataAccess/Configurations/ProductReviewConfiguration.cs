using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for ProductReview entity.
/// </summary>
public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    /// <summary>Configures the ProductReview entity.</summary>
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Title).HasMaxLength(200);
        builder.Property(r => r.Comment).HasMaxLength(2000);
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
