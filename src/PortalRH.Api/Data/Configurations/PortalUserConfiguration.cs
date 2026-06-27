using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalRH.Api.Models;

namespace PortalRH.Api.Data.Configurations;

public class PortalUserConfiguration : IEntityTypeConfiguration<PortalUser>
{
    public void Configure(EntityTypeBuilder<PortalUser> builder)
    {
        builder.ToTable("portal_users");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Login)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(item => item.SamAccountName)
            .HasMaxLength(120);

        builder.Property(item => item.UserPrincipalName)
            .HasMaxLength(200);

        builder.Property(item => item.Email)
            .HasMaxLength(200);

        builder.Property(item => item.DisplayName)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(item => item.Department)
            .HasMaxLength(180);

        builder.Property(item => item.Title)
            .HasMaxLength(180);

        builder.Property(item => item.DistinguishedName)
            .HasColumnType("text");

        builder.Property(item => item.ManagerDisplayName)
            .HasMaxLength(180);

        builder.Property(item => item.ManagerDistinguishedName)
            .HasColumnType("text");

        builder.Property(item => item.Role)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(item => item.ModulePermissionsJson)
            .HasColumnType("text");

        builder.Property(item => item.LastKnownIpAddress)
            .HasMaxLength(80);

        builder.Property(item => item.LastOrigin)
            .HasMaxLength(240);

        builder.Property(item => item.PasswordHash)
            .HasMaxLength(512);

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder.Property(item => item.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(item => item.Login)
            .IsUnique();
    }
}
