using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>
/// EF Core configuration for AuditLog entity.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    /// <summary>Configures the AuditLog entity.</summary>
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.UserId).HasMaxLength(450);
        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(450);
        builder.Property(a => a.IpAddress).HasMaxLength(50);
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
