namespace PortalRH.Api.Models;

public class CommunicationInteractionAuditLog
{
    public Guid Id { get; set; }
    public Guid CommunicationId { get; set; }
    public Guid PortalUserId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string ActorLogin { get; set; } = string.Empty;
    public string ActorDisplayName { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? Origin { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Communication? Communication { get; set; }
    public PortalUser? PortalUser { get; set; }
}
