using System.ComponentModel.DataAnnotations;

namespace PortalLioConnecta.Api.Contracts.Admin.Polls;

public class UpsertPollOptionRequest
{
    public Guid? Id { get; set; }

    [Required]
    [MaxLength(240)]
    public string Label { get; set; } = string.Empty;
}
