namespace PortalRH.Api.Contracts.MoodSurvey;

public sealed record MoodSurveyAuditLogDto(
    Guid Id,
    Guid PortalUserId,
    string PortalUserDisplayName,
    string? Department,
    string OptionKey,
    string OptionLabel,
    string OptionEmoji,
    string ActionType,
    string ActionTypeLabel,
    string? IpAddress,
    string? Origin,
    DateOnly SurveyDate,
    DateTime CreatedAtUtc);
