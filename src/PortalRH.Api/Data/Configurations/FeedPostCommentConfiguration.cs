using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalRH.Api.Models;

namespace PortalRH.Api.Data.Configurations;

public class FeedPostCommentConfiguration : IEntityTypeConfiguration<FeedPostComment>
{
    public void Configure(EntityTypeBuilder<FeedPostComment> builder)
    {
        builder.ToTable("feed_post_comments");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Text).HasMaxLength(2000).IsRequired();

        builder.HasIndex(item => new { item.FeedPostId, item.CreatedAtUtc });

        builder.HasOne(item => item.FeedPost)
            .WithMany(item => item.Comments)
            .HasForeignKey(item => item.FeedPostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.PortalUser)
            .WithMany()
            .HasForeignKey(item => item.PortalUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
