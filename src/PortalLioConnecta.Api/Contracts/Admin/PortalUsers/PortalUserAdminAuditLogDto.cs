namespace PortalLioConnecta.Api.Contracts.Admin.PortalUsers;

public sealed record PortalUserAdminAuditLogDto(
    Guid Id,
    Guid PortalUserId,
    string PortalUserDisplayName,
    string ActionType,
    string ActorUsername,
    string ActorDisplayName,
    string ActorRole,
    string? PreviousValue,
    string? NewValue,
    string? Notes,
    DateTime CreatedAtUtc);
