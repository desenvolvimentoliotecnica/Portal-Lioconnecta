namespace PortalLioConnecta.Api.Models;

public class PortalSession
{
    public Guid Id { get; set; }
    public Guid PortalUserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }

    public PortalUser PortalUser { get; set; } = null!;
}
