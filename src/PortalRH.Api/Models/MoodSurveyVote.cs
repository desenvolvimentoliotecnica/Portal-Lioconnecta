namespace PortalRH.Api.Models;

public class MoodSurveyVote
{
    public Guid Id { get; set; }
    public Guid PortalUserId { get; set; }
    public string OptionKey { get; set; } = string.Empty;
    public DateOnly SurveyDate { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? FeedbackMessageId { get; set; }
    public string? IpAddress { get; set; }
    public string? Origin { get; set; }

    public PortalUser? PortalUser { get; set; }
    public MoodSurveyFeedbackMessage? FeedbackMessage { get; set; }
}
