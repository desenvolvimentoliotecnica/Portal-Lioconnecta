namespace PortalRH.Api.Contracts.Notifications;

public record NotificationSummaryDto(
    int TotalCount,
    int UnreadCount,
    int ReadCount,
    IReadOnlyDictionary<string, int> CategoryCounts);
