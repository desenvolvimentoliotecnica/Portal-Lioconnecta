namespace PortalRH.Api.Contracts.MoodSurvey;

public sealed record MoodSurveyFeedbackMessageDto(
    Guid Id,
    string OptionKey,
    string OptionLabel,
    string OptionEmoji,
    string Message,
    int SortOrder,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record MoodSurveyFeedbackMessageListResponse(
    IReadOnlyList<MoodSurveyFeedbackMessageDto> Items,
    IReadOnlyList<MoodSurveyFeedbackOptionSummaryDto> OptionSummaries);

public sealed record MoodSurveyFeedbackOptionSummaryDto(
    string OptionKey,
    string OptionLabel,
    string OptionEmoji,
    int TotalMessages,
    int ActiveMessages);

public sealed class UpsertMoodSurveyFeedbackMessageRequest
{
    public string OptionKey { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
