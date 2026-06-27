namespace PortalRH.Api.Models;

public class FeedPostComment
{
    public Guid Id { get; set; }
    public Guid FeedPostId { get; set; }
    public Guid PortalUserId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? IpAddress { get; set; }
    public string? Origin { get; set; }

    public FeedPost? FeedPost { get; set; }
    public PortalUser? PortalUser { get; set; }
    public ICollection<FeedPostCommentMention> Mentions { get; set; } = new List<FeedPostCommentMention>();
}
