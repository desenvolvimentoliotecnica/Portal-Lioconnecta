namespace PortalRH.Api.Models;

public class PortalUser
{
    public Guid Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string? SamAccountName { get; set; }
    public string? UserPrincipalName { get; set; }
    public string? Email { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Title { get; set; }
    public string? DistinguishedName { get; set; }
    public string? ManagerDisplayName { get; set; }
    public string? ManagerDistinguishedName { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? ModulePermissionsJson { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
    public DateTime? LastFailedLoginAtUtc { get; set; }
    public int FailedLoginCount { get; set; }
    public string? LastKnownIpAddress { get; set; }
    public string? LastOrigin { get; set; }
    public string? PasswordHash { get; set; }

    public ICollection<PortalSession> Sessions { get; set; } = new List<PortalSession>();
    public ICollection<PortalUserLoginEvent> LoginEvents { get; set; } = new List<PortalUserLoginEvent>();
    public ICollection<PortalUserAdminAuditLog> AuditLogs { get; set; } = new List<PortalUserAdminAuditLog>();
}
