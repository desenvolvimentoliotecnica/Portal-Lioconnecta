namespace PortalRH.Api.Models;

public class Notification
{
    public Guid Id { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime PublishedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<PortalUserNotificationRead> Reads { get; set; } = new List<PortalUserNotificationRead>();
}
