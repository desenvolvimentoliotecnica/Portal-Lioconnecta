using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalRH.Api.Models;

namespace PortalRH.Api.Data.Configurations;

public class FeedPostConfiguration : IEntityTypeConfiguration<FeedPost>
{
    public void Configure(EntityTypeBuilder<FeedPost> builder)
    {
        builder.ToTable("feed_posts");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Text)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(item => item.IpAddress)
            .HasMaxLength(64);

        builder.Property(item => item.Origin)
            .HasMaxLength(256);

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(item => item.PortalUserId);
        builder.HasIndex(item => item.CreatedAtUtc);
        builder.HasIndex(item => item.DeletedAtUtc);

        builder.HasOne(item => item.PortalUser)
            .WithMany()
            .HasForeignKey(item => item.PortalUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
