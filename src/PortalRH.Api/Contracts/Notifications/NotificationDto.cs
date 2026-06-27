namespace PortalRH.Api.Contracts.Notifications;

public record NotificationDto(
    Guid Id,
    string Category,
    string Title,
    string Message,
    string Tone,
    string Icon,
    string TargetUrl,
    string Audience,
    string SourceType,
    Guid SourceId,
    DateTime PublishedAtUtc,
    bool IsRead,
    DateTime? ReadAtUtc);
