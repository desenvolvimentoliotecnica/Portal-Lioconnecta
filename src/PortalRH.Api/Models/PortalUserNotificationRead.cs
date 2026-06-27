namespace PortalRH.Api.Models;

public class PortalUserNotificationRead
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public Guid PortalUserId { get; set; }
    public DateTime ReadAtUtc { get; set; }

    public Notification Notification { get; set; } = null!;
    public PortalUser PortalUser { get; set; } = null!;
}
