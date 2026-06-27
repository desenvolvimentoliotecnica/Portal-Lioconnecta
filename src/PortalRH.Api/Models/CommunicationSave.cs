namespace PortalRH.Api.Models;

public class CommunicationSave
{
    public Guid Id { get; set; }
    public Guid CommunicationId { get; set; }
    public Guid PortalUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? IpAddress { get; set; }
    public string? Origin { get; set; }

    public Communication? Communication { get; set; }
    public PortalUser? PortalUser { get; set; }
}
