using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Data.Configurations;

public class MoodSurveyFeedbackMessageConfiguration : IEntityTypeConfiguration<MoodSurveyFeedbackMessage>
{
    public void Configure(EntityTypeBuilder<MoodSurveyFeedbackMessage> builder)
    {
        builder.ToTable("mood_survey_feedback_messages");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.OptionKey)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(item => item.Message)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(item => item.SortOrder)
            .IsRequired();

        builder.Property(item => item.IsActive)
            .IsRequired();

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder.Property(item => item.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(item => item.OptionKey);
        builder.HasIndex(item => new { item.OptionKey, item.IsActive });
    }
}
