namespace PortalLioConnecta.Api.Models;

public class PollOption
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public string Label { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public Poll? Poll { get; set; }
    public ICollection<PollVote> Votes { get; set; } = [];
}
