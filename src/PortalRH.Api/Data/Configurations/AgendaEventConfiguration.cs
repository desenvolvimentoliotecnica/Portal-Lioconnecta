using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalRH.Api.Models;

namespace PortalRH.Api.Data.Configurations;

public class AgendaEventConfiguration : IEntityTypeConfiguration<AgendaEvent>
{
    public void Configure(EntityTypeBuilder<AgendaEvent> builder)
    {
        builder.ToTable("agenda_events");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Title)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(item => item.Description)
            .HasMaxLength(500);

        builder.Property(item => item.Location)
            .HasMaxLength(160);

        builder.Property(item => item.Source)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(item => item.Audience)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(item => item.StartAtUtc)
            .IsRequired();

        builder.Property(item => item.EndAtUtc)
            .IsRequired();

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder.Property(item => item.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(item => item.StartAtUtc);
        builder.HasIndex(item => item.IsActive);
        builder.HasIndex(item => item.PortalUserId);

        builder.HasOne(item => item.PortalUser)
            .WithMany()
            .HasForeignKey(item => item.PortalUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
