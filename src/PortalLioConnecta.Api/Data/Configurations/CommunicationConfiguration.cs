using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Data.Configurations;

public class CommunicationConfiguration : IEntityTypeConfiguration<Communication>
{
    public void Configure(EntityTypeBuilder<Communication> builder)
    {
        builder.ToTable("communications");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Slug)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(item => item.Category)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(item => item.Priority)
            .HasMaxLength(80)
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

        builder.Property(item => item.Audience)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(item => item.Channel)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(item => item.Status)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(item => item.AttachmentLabel)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(item => item.Owner)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(item => item.ImageUrl)
            .HasColumnType("text");

        builder.Property(item => item.PublishedAt)
            .IsRequired();

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder.Property(item => item.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(item => item.Slug)
            .IsUnique();

        builder.HasIndex(item => item.PublishedAt);
    }
}
