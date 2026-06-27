namespace PortalRH.Api.Models;

public class FeedPost
{
    public Guid Id { get; set; }
    public Guid PortalUserId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? IpAddress { get; set; }
    public string? Origin { get; set; }

    public PortalUser? PortalUser { get; set; }
    public ICollection<FeedPostMedia> Media { get; set; } = new List<FeedPostMedia>();
    public ICollection<FeedPostComment> Comments { get; set; } = new List<FeedPostComment>();
    public ICollection<FeedPostMention> Mentions { get; set; } = new List<FeedPostMention>();
}
