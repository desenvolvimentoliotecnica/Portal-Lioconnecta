namespace PortalRH.Api.Models;

public class MoodSurveyFeedbackMessage
{
    public Guid Id { get; set; }
    public string OptionKey { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
