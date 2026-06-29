namespace PortalLioConnecta.Api.Models;

public class PollVote
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public Guid PollOptionId { get; set; }
    public Guid PortalUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Poll? Poll { get; set; }
    public PollOption? PollOption { get; set; }
    public PortalUser? PortalUser { get; set; }
}
