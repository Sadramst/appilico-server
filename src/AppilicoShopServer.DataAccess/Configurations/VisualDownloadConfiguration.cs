using AppilicoShopServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppilicoShopServer.DataAccess.Configurations;

/// <summary>EF Core configuration for the VisualDownload entity.</summary>
public class VisualDownloadConfiguration : IEntityTypeConfiguration<VisualDownload>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<VisualDownload> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.UserId).IsRequired();
        builder.Property(d => d.IPAddress).HasMaxLength(50);

        builder.HasOne(d => d.Visual)
            .WithMany(v => v.Downloads)
            .HasForeignKey(d => d.VisualId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
