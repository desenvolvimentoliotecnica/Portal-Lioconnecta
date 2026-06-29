namespace PortalLioConnecta.Api.Interfaces;

public interface IFeedInteractionNotificationService
{
    Task RegisterLikeAsync(
        Guid recipientPortalUserId,
        Guid actorPortalUserId,
        string actorDisplayName,
        Guid feedPostId,
        Guid likeId,
        CancellationToken cancellationToken);

    Task RegisterShareAsync(
        Guid recipientPortalUserId,
        Guid actorPortalUserId,
        string actorDisplayName,
        Guid feedPostId,
        Guid shareId,
        CancellationToken cancellationToken);

    Task RegisterSaveAsync(
        Guid recipientPortalUserId,
        Guid actorPortalUserId,
        string actorDisplayName,
        Guid feedPostId,
        Guid saveId,
        CancellationToken cancellationToken);

    Task RegisterCommentAsync(
        Guid recipientPortalUserId,
        Guid actorPortalUserId,
        string actorDisplayName,
        Guid feedPostId,
        Guid commentId,
        string commentPreview,
        CancellationToken cancellationToken);

    Task RegisterMentionAsync(
        Guid mentionedPortalUserId,
        Guid actorPortalUserId,
        string actorDisplayName,
        Guid feedPostId,
        Guid commentMentionId,
        CancellationToken cancellationToken);

    Task DeactivateAsync(string sourceType, Guid sourceId, CancellationToken cancellationToken);
}
