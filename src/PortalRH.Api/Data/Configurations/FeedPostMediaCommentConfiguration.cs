using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalRH.Api.Models;

namespace PortalRH.Api.Data.Configurations;

public class FeedPostMediaCommentConfiguration : IEntityTypeConfiguration<FeedPostMediaComment>
{
    public void Configure(EntityTypeBuilder<FeedPostMediaComment> builder)
    {
        builder.ToTable("feed_post_media_comments");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Text).HasMaxLength(1000).IsRequired();

        builder.HasIndex(item => new { item.FeedPostMediaId, item.CreatedAtUtc });

        builder.HasOne(item => item.FeedPostMedia)
            .WithMany(item => item.Comments)
            .HasForeignKey(item => item.FeedPostMediaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.PortalUser)
            .WithMany()
            .HasForeignKey(item => item.PortalUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
