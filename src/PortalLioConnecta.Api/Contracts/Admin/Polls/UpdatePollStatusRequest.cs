using System.ComponentModel.DataAnnotations;

namespace PortalLioConnecta.Api.Contracts.Admin.Polls;

public class UpdatePollStatusRequest
{
    [Required]
    [MaxLength(40)]
    public string Status { get; set; } = string.Empty;
}
