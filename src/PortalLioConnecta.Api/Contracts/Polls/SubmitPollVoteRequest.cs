using System.ComponentModel.DataAnnotations;

namespace PortalLioConnecta.Api.Contracts.Polls;

public class SubmitPollVoteRequest
{
    [Required]
    [MinLength(1)]
    public List<Guid> OptionIds { get; set; } = [];
}
