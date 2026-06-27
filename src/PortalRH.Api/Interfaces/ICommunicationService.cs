using PortalRH.Api.Contracts.Communications;

namespace PortalRH.Api.Interfaces;

public interface ICommunicationService
{
    Task<IReadOnlyList<CommunicationDto>> GetAllAsync(Guid? portalUserId, CancellationToken cancellationToken);
    Task<CommunicationDto?> GetByIdAsync(Guid id, Guid? portalUserId, CancellationToken cancellationToken);
    Task<CommunicationDto?> GetBySlugAsync(string slug, Guid? portalUserId, CancellationToken cancellationToken);
    Task<CommunicationDto> CreateAsync(UpsertCommunicationRequest request, CancellationToken cancellationToken);
    Task<CommunicationDto?> UpdateAsync(Guid id, UpsertCommunicationRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<CommunicationLikeResponse?> ToggleLikeAsync(
        Guid communicationId,
        Guid portalUserId,
        CommunicationAuditContext auditContext,
        CancellationToken cancellationToken);
    Task<CommunicationShareResponse?> ToggleShareAsync(
        Guid communicationId,
        Guid portalUserId,
        CommunicationAuditContext auditContext,
        CancellationToken cancellationToken);
    Task<CommunicationSaveResponse?> ToggleSaveAsync(
        Guid communicationId,
        Guid portalUserId,
        CommunicationAuditContext auditContext,
        CancellationToken cancellationToken);
}

public sealed record CommunicationAuditContext(
    string ActorLogin,
    string ActorDisplayName,
    string? IpAddress,
    string? Origin,
    string? UserAgent);
