using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Data.Configurations;

public class PortalUserLoginEventConfiguration : IEntityTypeConfiguration<PortalUserLoginEvent>
{
    public void Configure(EntityTypeBuilder<PortalUserLoginEvent> builder)
    {
        builder.ToTable("portal_user_login_events");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Login)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(item => item.DisplayNameSnapshot)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(item => item.EmailSnapshot)
            .HasMaxLength(200);

        builder.Property(item => item.DepartmentSnapshot)
            .HasMaxLength(180);

        builder.Property(item => item.AuthenticationProvider)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(item => item.EventType)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(item => item.FailureReason)
            .HasMaxLength(240);

        builder.Property(item => item.IpAddress)
            .HasMaxLength(80);

        builder.Property(item => item.Origin)
            .HasMaxLength(240);

        builder.Property(item => item.UserAgent)
            .HasMaxLength(400);

        builder.Property(item => item.LoggedAtUtc)
            .IsRequired();

        builder.HasIndex(item => item.PortalUserId);
        builder.HasIndex(item => item.Login);
        builder.HasIndex(item => item.LoggedAtUtc);

        builder.HasOne(item => item.PortalUser)
            .WithMany(item => item.LoginEvents)
            .HasForeignKey(item => item.PortalUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
