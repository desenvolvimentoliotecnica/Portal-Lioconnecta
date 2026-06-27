using PortalRH.Api.Contracts.Notifications;

namespace PortalRH.Api.Interfaces;

public interface INotificationService
{
    Task<NotificationListResponse> GetForUserAsync(Guid portalUserId, CancellationToken cancellationToken);
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid portalUserId, CancellationToken cancellationToken);
    Task<int> MarkAllAsReadAsync(Guid portalUserId, CancellationToken cancellationToken);
}
