using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Data.Configurations;

public class PollConfiguration : IEntityTypeConfiguration<Poll>
{
    public void Configure(EntityTypeBuilder<Poll> builder)
    {
        builder.ToTable("polls");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Slug)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(item => item.Title)
            .HasMaxLength(240)
            .IsRequired();

        builder.Property(item => item.Summary)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(item => item.Body)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(item => item.ImageUrl)
            .HasMaxLength(500);

        builder.Property(item => item.AttachmentLabel)
            .HasMaxLength(120);

        builder.Property(item => item.AttachmentUrl)
            .HasMaxLength(500);

        builder.Property(item => item.Audience)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(item => item.Status)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(item => item.ResultsVisibility)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder.Property(item => item.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(item => item.Slug)
            .IsUnique();

        builder.HasIndex(item => item.Status);
        builder.HasIndex(item => item.PublishedAtUtc);
        builder.HasMany(item => item.Options)
            .WithOne(item => item.Poll)
            .HasForeignKey(item => item.PollId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(item => item.Votes)
            .WithOne(item => item.Poll)
            .HasForeignKey(item => item.PollId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
