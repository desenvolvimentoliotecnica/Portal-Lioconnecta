namespace PortalRH.Api.Contracts.Notifications;

public record NotificationListResponse(
    IReadOnlyList<NotificationDto> Items,
    NotificationSummaryDto Summary);
