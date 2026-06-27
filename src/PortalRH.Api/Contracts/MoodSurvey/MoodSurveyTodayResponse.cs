namespace PortalRH.Api.Contracts.MoodSurvey;

public record MoodSurveyTodayResponse(
    string Title,
    DateOnly SurveyDate,
    bool HasVoted,
    string? SelectedOptionKey,
    string? ThankYouMessage,
    IReadOnlyList<MoodSurveyOptionDto> Items);

public record MoodSurveyOptionDto(
    string Key,
    string Emoji,
    string Label,
    string Rank,
    int VoteCount);
