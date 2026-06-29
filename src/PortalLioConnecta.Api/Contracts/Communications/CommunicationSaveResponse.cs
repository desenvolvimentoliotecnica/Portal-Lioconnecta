namespace PortalLioConnecta.Api.Contracts.Communications;

public sealed record CommunicationSaveResponse(
    Guid CommunicationId,
    bool HasSaved);
