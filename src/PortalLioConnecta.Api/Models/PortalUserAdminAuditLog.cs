namespace PortalLioConnecta.Api.Models;

public class PortalUserAdminAuditLog
{
    public Guid Id { get; set; }
    public Guid PortalUserId { get; set; }
    public Guid? AdminUserId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string ActorUsername { get; set; } = string.Empty;
    public string ActorDisplayName { get; set; } = string.Empty;
    public string ActorRole { get; set; } = string.Empty;
    public string? PreviousValue { get; set; }
    public string? NewValue { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public PortalUser PortalUser { get; set; } = null!;
    public AdminUser? AdminUser { get; set; }
}
