namespace PortalLioConnecta.Api.Models;

public class Poll
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? AttachmentLabel { get; set; }
    public string? AttachmentUrl { get; set; }
    public string Audience { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool AllowMultipleChoices { get; set; }
    public string ResultsVisibility { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime? ClosesAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<PollOption> Options { get; set; } = [];
    public ICollection<PollVote> Votes { get; set; } = [];
}
