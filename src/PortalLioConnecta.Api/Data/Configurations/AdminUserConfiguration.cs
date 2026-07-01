using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Data.Configurations;

public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("admin_users");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Username)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(item => item.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(item => item.DisplayName)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(item => item.Role)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(item => item.IsActive)
            .IsRequired();

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder.Property(item => item.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(item => item.Username)
            .IsUnique();
    }
}
