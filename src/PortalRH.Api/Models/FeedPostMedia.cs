namespace PortalRH.Api.Models;

public class FeedPostMedia
{
    public Guid Id { get; set; }
    public Guid FeedPostId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AspectRatio { get; set; } = "free";
    public int SortOrder { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public FeedPost? FeedPost { get; set; }
    public ICollection<FeedPostMediaComment> Comments { get; set; } = new List<FeedPostMediaComment>();
}
