using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalRH.Api.Models;

namespace PortalRH.Api.Data.Configurations;

public class FeedPostMediaConfiguration : IEntityTypeConfiguration<FeedPostMedia>
{
    public void Configure(EntityTypeBuilder<FeedPostMedia> builder)
    {
        builder.ToTable("feed_post_media");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Url).HasMaxLength(2048).IsRequired();
        builder.Property(item => item.Description).HasMaxLength(500).IsRequired();
        builder.Property(item => item.AspectRatio).HasMaxLength(16).IsRequired();

        builder.HasIndex(item => new { item.FeedPostId, item.SortOrder });

        builder.HasOne(item => item.FeedPost)
            .WithMany(item => item.Media)
            .HasForeignKey(item => item.FeedPostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
