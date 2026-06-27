using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalRH.Api.Models;

namespace PortalRH.Api.Data.Configurations;

public class FeedPostAuditLogConfiguration : IEntityTypeConfiguration<FeedPostAuditLog>
{
    public void Configure(EntityTypeBuilder<FeedPostAuditLog> builder)
    {
        builder.ToTable("feed_post_audit_logs");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.ActionType)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(item => item.ActorLogin)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(item => item.ActorDisplayName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(item => item.IpAddress)
            .HasMaxLength(64);

        builder.Property(item => item.Origin)
            .HasMaxLength(256);

        builder.Property(item => item.UserAgent)
            .HasMaxLength(512);

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(item => item.FeedPostId);
        builder.HasIndex(item => item.PortalUserId);
        builder.HasIndex(item => item.CreatedAtUtc);

        builder.HasOne(item => item.FeedPost)
            .WithMany()
            .HasForeignKey(item => item.FeedPostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.PortalUser)
            .WithMany()
            .HasForeignKey(item => item.PortalUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
