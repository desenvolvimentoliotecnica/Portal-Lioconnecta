using System.ComponentModel.DataAnnotations;

namespace PortalRH.Api.Contracts.Admin.MicrosoftGraph;

public class UpsertMicrosoftGraphConfigurationRequest
{
    public bool IsEnabled { get; set; }

    [Required]
    [MaxLength(80)]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string ClientId { get; set; } = string.Empty;

    public string? ClientSecret { get; set; }

    [Required]
    [MaxLength(40)]
    public string UserIdentifier { get; set; } = string.Empty;
}
