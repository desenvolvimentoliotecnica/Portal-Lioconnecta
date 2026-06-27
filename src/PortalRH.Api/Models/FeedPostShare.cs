namespace PortalRH.Api.Models;

public class FeedPostShare
{
    public Guid Id { get; set; }
    public Guid FeedPostId { get; set; }
    public Guid PortalUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? IpAddress { get; set; }
    public string? Origin { get; set; }

    public FeedPost? FeedPost { get; set; }
    public PortalUser? PortalUser { get; set; }
}
