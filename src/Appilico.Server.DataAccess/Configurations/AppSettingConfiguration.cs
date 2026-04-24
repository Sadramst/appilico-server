using Appilico.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appilico.Server.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for AppSetting entity.
/// </summary>
public class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    /// <summary>Configures the AppSetting entity.</summary>
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Key).HasMaxLength(100).IsRequired();
        builder.HasIndex(s => s.Key).IsUnique();
        builder.Property(s => s.Value).HasMaxLength(2000).IsRequired();
        builder.Property(s => s.Group).HasMaxLength(100);
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
