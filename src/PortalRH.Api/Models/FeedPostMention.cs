namespace PortalRH.Api.Models;

public class FeedPostMention
{
    public Guid Id { get; set; }
    public Guid FeedPostId { get; set; }
    public Guid MentionedPortalUserId { get; set; }

    public FeedPost? FeedPost { get; set; }
    public PortalUser? MentionedPortalUser { get; set; }
}
