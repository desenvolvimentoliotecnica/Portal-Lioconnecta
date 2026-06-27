using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalRH.Api.Models;

namespace PortalRH.Api.Data.Configurations;

public class FeedPostMentionConfiguration : IEntityTypeConfiguration<FeedPostMention>
{
    public void Configure(EntityTypeBuilder<FeedPostMention> builder)
    {
        builder.ToTable("feed_post_mentions");

        builder.HasKey(item => item.Id);

        builder.HasIndex(item => new { item.FeedPostId, item.MentionedPortalUserId })
            .IsUnique();

        builder.HasOne(item => item.FeedPost)
            .WithMany(item => item.Mentions)
            .HasForeignKey(item => item.FeedPostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.MentionedPortalUser)
            .WithMany()
            .HasForeignKey(item => item.MentionedPortalUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
