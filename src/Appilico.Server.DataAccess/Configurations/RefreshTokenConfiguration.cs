using Appilico.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appilico.Server.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for RefreshToken entity.
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <summary>Configures the RefreshToken entity.</summary>
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Token).HasMaxLength(500).IsRequired();
        builder.Property(r => r.CreatedByIp).HasMaxLength(50);
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
