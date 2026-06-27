namespace PortalRH.Api.Models;

public class FeedPostCommentMention
{
    public Guid Id { get; set; }
    public Guid FeedPostCommentId { get; set; }
    public Guid MentionedPortalUserId { get; set; }

    public FeedPostComment? FeedPostComment { get; set; }
    public PortalUser? MentionedPortalUser { get; set; }
}
