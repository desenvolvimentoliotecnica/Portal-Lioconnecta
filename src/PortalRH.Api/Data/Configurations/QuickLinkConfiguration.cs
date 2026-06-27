using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalRH.Api.Models;

namespace PortalRH.Api.Data.Configurations;

public class QuickLinkConfiguration : IEntityTypeConfiguration<QuickLink>
{
    public void Configure(EntityTypeBuilder<QuickLink> builder)
    {
        builder.ToTable("quick_links");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Label).HasMaxLength(160).IsRequired();
        builder.Property(item => item.ShortLabel).HasMaxLength(16).IsRequired();
        builder.Property(item => item.ClassName).HasMaxLength(32).IsRequired();
        builder.Property(item => item.Url).HasMaxLength(2048).IsRequired();
        builder.Property(item => item.Audience).HasMaxLength(120);

        builder.HasIndex(item => new { item.IsActive, item.SortOrder });
    }
}
