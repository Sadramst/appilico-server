using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for AppUser entity.
/// </summary>
public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    /// <summary>Configures the AppUser entity.</summary>
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Avatar).HasMaxLength(500);

        builder.HasOne(u => u.Customer)
            .WithOne(c => c.User)
            .HasForeignKey<Customer>(c => c.UserId);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId);
    }
}
