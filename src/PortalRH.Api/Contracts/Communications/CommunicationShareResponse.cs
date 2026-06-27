namespace PortalRH.Api.Contracts.Communications;

public sealed record CommunicationShareResponse(
    Guid CommunicationId,
    int ShareCount,
    bool HasShared);
