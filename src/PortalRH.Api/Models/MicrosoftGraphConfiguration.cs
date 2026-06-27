namespace PortalRH.Api.Models;

public class MicrosoftGraphConfiguration
{
    public Guid Id { get; set; }
    public bool IsEnabled { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string? ClientSecretProtected { get; set; }
    public string UserIdentifier { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
