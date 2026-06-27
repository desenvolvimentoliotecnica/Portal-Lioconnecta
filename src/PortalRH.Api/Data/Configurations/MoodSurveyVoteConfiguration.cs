using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalRH.Api.Models;

namespace PortalRH.Api.Data.Configurations;

public class MoodSurveyVoteConfiguration : IEntityTypeConfiguration<MoodSurveyVote>
{
    public void Configure(EntityTypeBuilder<MoodSurveyVote> builder)
    {
        builder.ToTable("mood_survey_votes");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.OptionKey)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(item => item.SurveyDate)
            .IsRequired();

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder.Property(item => item.IpAddress)
            .HasMaxLength(64);

        builder.Property(item => item.Origin)
            .HasMaxLength(256);

        builder.HasIndex(item => new { item.PortalUserId, item.SurveyDate })
            .IsUnique();

        builder.HasIndex(item => item.SurveyDate);

        builder.HasOne(item => item.PortalUser)
            .WithMany()
            .HasForeignKey(item => item.PortalUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.FeedbackMessage)
            .WithMany()
            .HasForeignKey(item => item.FeedbackMessageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
