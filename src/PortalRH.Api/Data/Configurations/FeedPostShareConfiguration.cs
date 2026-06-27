using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalRH.Api.Models;

namespace PortalRH.Api.Data.Configurations;

public class FeedPostShareConfiguration : IEntityTypeConfiguration<FeedPostShare>
{
    public void Configure(EntityTypeBuilder<FeedPostShare> builder)
    {
        builder.ToTable("feed_post_shares");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.IpAddress)
            .HasMaxLength(64);

        builder.Property(item => item.Origin)
            .HasMaxLength(256);

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(item => new { item.FeedPostId, item.PortalUserId })
            .IsUnique();

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
