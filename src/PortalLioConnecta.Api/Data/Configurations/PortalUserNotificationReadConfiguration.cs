using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Data.Configurations;

public class PortalUserNotificationReadConfiguration : IEntityTypeConfiguration<PortalUserNotificationRead>
{
    public void Configure(EntityTypeBuilder<PortalUserNotificationRead> builder)
    {
        builder.ToTable("portal_user_notification_reads");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.ReadAtUtc)
            .IsRequired();

        builder.HasIndex(item => new { item.NotificationId, item.PortalUserId })
            .IsUnique();

        builder.HasOne(item => item.PortalUser)
            .WithMany()
            .HasForeignKey(item => item.PortalUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
