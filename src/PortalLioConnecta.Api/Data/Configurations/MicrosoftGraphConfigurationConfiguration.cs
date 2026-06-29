using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Data.Configurations;

public class MicrosoftGraphConfigurationConfiguration : IEntityTypeConfiguration<MicrosoftGraphConfiguration>
{
    public void Configure(EntityTypeBuilder<MicrosoftGraphConfiguration> builder)
    {
        builder.ToTable("microsoft_graph_configurations");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.TenantId)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(item => item.ClientId)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(item => item.ClientSecretProtected)
            .HasColumnType("text");

        builder.Property(item => item.UserIdentifier)
            .HasMaxLength(40)
            .IsRequired();
    }
}
