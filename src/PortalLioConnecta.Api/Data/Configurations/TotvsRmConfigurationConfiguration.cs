using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Data.Configurations;

public class TotvsRmConfigurationConfiguration : IEntityTypeConfiguration<TotvsRmConfiguration>
{
    public void Configure(EntityTypeBuilder<TotvsRmConfiguration> builder)
    {
        builder.ToTable("totvs_rm_configurations");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Server)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(item => item.Database)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(item => item.UserName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(item => item.PasswordProtected)
            .HasColumnType("text");

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder.Property(item => item.UpdatedAtUtc)
            .IsRequired();
    }
}
