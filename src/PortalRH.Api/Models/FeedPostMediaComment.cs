namespace PortalRH.Api.Models;

public class FeedPostMediaComment
{
    public Guid Id { get; set; }
    public Guid FeedPostMediaId { get; set; }
    public Guid PortalUserId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? IpAddress { get; set; }
    public string? Origin { get; set; }

    public FeedPostMedia? FeedPostMedia { get; set; }
    public PortalUser? PortalUser { get; set; }
}
