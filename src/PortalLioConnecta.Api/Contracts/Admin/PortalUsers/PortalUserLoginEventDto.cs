namespace PortalLioConnecta.Api.Contracts.Admin.PortalUsers;

public sealed record PortalUserLoginEventDto(
    Guid Id,
    Guid? PortalUserId,
    string Login,
    string DisplayName,
    string? Department,
    string AuthenticationProvider,
    string EventType,
    string EventTypeLabel,
    bool IsSuccess,
    string? FailureReason,
    string? IpAddress,
    string? Origin,
    DateTime LoggedAtUtc);
