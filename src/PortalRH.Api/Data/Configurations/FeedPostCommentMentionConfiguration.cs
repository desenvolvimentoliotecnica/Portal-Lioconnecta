using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalRH.Api.Models;

namespace PortalRH.Api.Data.Configurations;

public class FeedPostCommentMentionConfiguration : IEntityTypeConfiguration<FeedPostCommentMention>
{
    public void Configure(EntityTypeBuilder<FeedPostCommentMention> builder)
    {
        builder.ToTable("feed_post_comment_mentions");

        builder.HasKey(item => item.Id);

        builder.HasIndex(item => new { item.FeedPostCommentId, item.MentionedPortalUserId })
            .IsUnique();

        builder.HasOne(item => item.FeedPostComment)
            .WithMany(item => item.Mentions)
            .HasForeignKey(item => item.FeedPostCommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.MentionedPortalUser)
            .WithMany()
            .HasForeignKey(item => item.MentionedPortalUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
