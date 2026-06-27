using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalRH.Api.Models;

namespace PortalRH.Api.Data.Configurations;

public class CommunicationSaveConfiguration : IEntityTypeConfiguration<CommunicationSave>
{
    public void Configure(EntityTypeBuilder<CommunicationSave> builder)
    {
        builder.ToTable("communication_saves");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.IpAddress)
            .HasMaxLength(64);

        builder.Property(item => item.Origin)
            .HasMaxLength(256);

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(item => new { item.CommunicationId, item.PortalUserId })
            .IsUnique();

        builder.HasIndex(item => item.PortalUserId);
        builder.HasIndex(item => item.CreatedAtUtc);

        builder.HasOne(item => item.Communication)
            .WithMany()
            .HasForeignKey(item => item.CommunicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.PortalUser)
            .WithMany()
            .HasForeignKey(item => item.PortalUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
