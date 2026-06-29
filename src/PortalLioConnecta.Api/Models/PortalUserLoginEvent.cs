namespace PortalLioConnecta.Api.Models;

public class PortalUserLoginEvent
{
    public Guid Id { get; set; }
    public Guid? PortalUserId { get; set; }
    public string Login { get; set; } = string.Empty;
    public string DisplayNameSnapshot { get; set; } = string.Empty;
    public string? EmailSnapshot { get; set; }
    public string? DepartmentSnapshot { get; set; }
    public string AuthenticationProvider { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; }
    public string? IpAddress { get; set; }
    public string? Origin { get; set; }
    public string? UserAgent { get; set; }
    public DateTime LoggedAtUtc { get; set; }

    public PortalUser? PortalUser { get; set; }
}
