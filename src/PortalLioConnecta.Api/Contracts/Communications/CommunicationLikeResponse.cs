namespace PortalLioConnecta.Api.Contracts.Communications;

public sealed record CommunicationLikeResponse(
    Guid CommunicationId,
    int LikeCount,
    bool HasLiked);
