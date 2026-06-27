namespace PortalRH.Api.Contracts.Communications;

public sealed record CommunicationSaveResponse(
    Guid CommunicationId,
    bool HasSaved);
