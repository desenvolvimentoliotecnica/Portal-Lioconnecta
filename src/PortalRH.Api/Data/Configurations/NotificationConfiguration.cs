using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalRH.Api.Models;

namespace PortalRH.Api.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.SourceType)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(item => item.Category)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(item => item.Title)
            .HasMaxLength(240)
            .IsRequired();

        builder.Property(item => item.Message)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(item => item.Tone)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(item => item.Icon)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(item => item.TargetUrl)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(item => item.Audience)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(item => item.PublishedAtUtc)
            .IsRequired();

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder.Property(item => item.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(item => new { item.SourceType, item.SourceId })
            .IsUnique();

        builder.HasIndex(item => item.PublishedAtUtc);
        builder.HasIndex(item => item.IsActive);

        builder.HasMany(item => item.Reads)
            .WithOne(item => item.Notification)
            .HasForeignKey(item => item.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
