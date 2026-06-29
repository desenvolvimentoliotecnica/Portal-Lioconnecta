using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Data.Configurations;

public class LdapConfigurationConfiguration : IEntityTypeConfiguration<LdapConfiguration>
{
    public void Configure(EntityTypeBuilder<LdapConfiguration> builder)
    {
        builder.ToTable("ldap_configurations");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Server)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(item => item.BaseDn)
            .HasMaxLength(240)
            .IsRequired();

        builder.Property(item => item.UserSearchBase)
            .HasMaxLength(240);

        builder.Property(item => item.NetbiosDomain)
            .HasMaxLength(120);

        builder.Property(item => item.LoginFormat)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(item => item.BindDn)
            .HasMaxLength(240);

        builder.Property(item => item.BindPasswordProtected)
            .HasColumnType("text");

        builder.Property(item => item.SearchFilter)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(item => item.DisplayNameAttribute)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder.Property(item => item.UpdatedAtUtc)
            .IsRequired();
    }
}
