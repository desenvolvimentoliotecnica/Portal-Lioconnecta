using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Data.Configurations;

public class PortalSessionConfiguration : IEntityTypeConfiguration<PortalSession>
{
    public void Configure(EntityTypeBuilder<PortalSession> builder)
    {
        builder.ToTable("portal_sessions");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Token)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder.Property(item => item.ExpiresAtUtc)
            .IsRequired();

        builder.HasIndex(item => item.Token)
            .IsUnique();

        builder.HasOne(item => item.PortalUser)
            .WithMany(item => item.Sessions)
            .HasForeignKey(item => item.PortalUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
