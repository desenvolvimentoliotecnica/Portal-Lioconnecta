using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Data.Configurations;

public class AdminSessionConfiguration : IEntityTypeConfiguration<AdminSession>
{
    public void Configure(EntityTypeBuilder<AdminSession> builder)
    {
        builder.ToTable("admin_sessions");

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

        builder.HasOne(item => item.AdminUser)
            .WithMany(item => item.Sessions)
            .HasForeignKey(item => item.AdminUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
