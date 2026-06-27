namespace PortalRH.Api.Models;

public class AgendaEvent
{
    public Guid Id { get; set; }
    public Guid? PortalUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public PortalUser? PortalUser { get; set; }
}
