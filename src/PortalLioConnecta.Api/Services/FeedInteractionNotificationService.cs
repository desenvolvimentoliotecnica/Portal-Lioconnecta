using Microsoft.EntityFrameworkCore;
using PortalLioConnecta.Api.Data;
using PortalLioConnecta.Api.Domain;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Services;

public class FeedInteractionNotificationService : IFeedInteractionNotificationService
{
    private const string TargetUrl = "#inicio";
    private const int MessagePreviewLength = 120;

    private readonly PortalLioConnectaDbContext _dbContext;

    public FeedInteractionNotificationService(PortalLioConnectaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task RegisterLikeAsync(
        Guid recipientPortalUserId,
        Guid actorPortalUserId,
        string actorDisplayName,
        Guid feedPostId,
        Guid likeId,
        CancellationToken cancellationToken)
    {
        if (recipientPortalUserId == actorPortalUserId)
        {
            return Task.CompletedTask;
        }

        return UpsertAsync(
            FeedNotificationSourceTypes.Like,
            likeId,
            recipientPortalUserId,
            actorPortalUserId,
            $"{actorDisplayName} curtiu sua publicacao",
            "Sua publicacao recebeu uma nova curtida.",
            "fa-solid fa-thumbs-up",
            "success",
            cancellationToken);
    }

    public Task RegisterShareAsync(
        Guid recipientPortalUserId,
        Guid actorPortalUserId,
        string actorDisplayName,
        Guid feedPostId,
        Guid shareId,
        CancellationToken cancellationToken)
    {
        if (recipientPortalUserId == actorPortalUserId)
        {
            return Task.CompletedTask;
        }

        return UpsertAsync(
            FeedNotificationSourceTypes.Share,
            shareId,
            recipientPortalUserId,
            actorPortalUserId,
            $"{actorDisplayName} compartilhou sua publicacao",
            "Sua publicacao foi compartilhada no feed.",
            "fa-solid fa-share-nodes",
            "info",
            cancellationToken);
    }

    public Task RegisterSaveAsync(
        Guid recipientPortalUserId,
        Guid actorPortalUserId,
        string actorDisplayName,
        Guid feedPostId,
        Guid saveId,
        CancellationToken cancellationToken)
    {
        if (recipientPortalUserId == actorPortalUserId)
        {
            return Task.CompletedTask;
        }

        return UpsertAsync(
            FeedNotificationSourceTypes.Save,
            saveId,
            recipientPortalUserId,
            actorPortalUserId,
            $"{actorDisplayName} salvou sua publicacao",
            "Sua publicacao foi adicionada aos salvos de um colega.",
            "fa-solid fa-bookmark",
            "info",
            cancellationToken);
    }

    public Task RegisterCommentAsync(
        Guid recipientPortalUserId,
        Guid actorPortalUserId,
        string actorDisplayName,
        Guid feedPostId,
        Guid commentId,
        string commentPreview,
        CancellationToken cancellationToken)
    {
        if (recipientPortalUserId == actorPortalUserId)
        {
            return Task.CompletedTask;
        }

        var preview = TrimPreview(commentPreview);

        return UpsertAsync(
            FeedNotificationSourceTypes.Comment,
            commentId,
            recipientPortalUserId,
            actorPortalUserId,
            $"{actorDisplayName} comentou sua publicacao",
            string.IsNullOrWhiteSpace(preview)
                ? "Sua publicacao recebeu um novo comentario."
                : preview,
            "fa-solid fa-comment",
            "info",
            cancellationToken);
    }

    public Task RegisterMentionAsync(
        Guid mentionedPortalUserId,
        Guid actorPortalUserId,
        string actorDisplayName,
        Guid feedPostId,
        Guid commentMentionId,
        CancellationToken cancellationToken)
    {
        if (mentionedPortalUserId == actorPortalUserId)
        {
            return Task.CompletedTask;
        }

        return UpsertAsync(
            FeedNotificationSourceTypes.Mention,
            commentMentionId,
            mentionedPortalUserId,
            actorPortalUserId,
            $"{actorDisplayName} mencionou voce em um comentario",
            "Voce foi mencionado em uma conversa do feed.",
            "fa-solid fa-at",
            "warning",
            cancellationToken);
    }

    public async Task DeactivateAsync(string sourceType, Guid sourceId, CancellationToken cancellationToken)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(
                item => item.SourceType == sourceType && item.SourceId == sourceId,
                cancellationToken);

        if (notification is null || !notification.IsActive)
        {
            return;
        }

        notification.IsActive = false;
        notification.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertAsync(
        string sourceType,
        Guid sourceId,
        Guid recipientPortalUserId,
        Guid actorPortalUserId,
        string title,
        string message,
        string icon,
        string tone,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(
                item => item.SourceType == sourceType && item.SourceId == sourceId,
                cancellationToken);

        if (notification is null)
        {
            _dbContext.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                SourceType = sourceType,
                SourceId = sourceId,
                Category = FeedNotificationCategories.FeedInteractions,
                Title = title,
                Message = message,
                Tone = tone,
                Icon = icon,
                TargetUrl = TargetUrl,
                Audience = "Usuario autenticado",
                RecipientPortalUserId = recipientPortalUserId,
                ActorPortalUserId = actorPortalUserId,
                IsActive = true,
                PublishedAtUtc = now,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
        }
        else
        {
            notification.Category = FeedNotificationCategories.FeedInteractions;
            notification.Title = title;
            notification.Message = message;
            notification.Tone = tone;
            notification.Icon = icon;
            notification.TargetUrl = TargetUrl;
            notification.Audience = "Usuario autenticado";
            notification.RecipientPortalUserId = recipientPortalUserId;
            notification.ActorPortalUserId = actorPortalUserId;
            notification.IsActive = true;
            notification.PublishedAtUtc = now;
            notification.UpdatedAtUtc = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string TrimPreview(string value)
    {
        var text = value?.Trim() ?? string.Empty;
        if (text.Length <= MessagePreviewLength)
        {
            return text;
        }

        return $"{text[..MessagePreviewLength].Trim()}...";
    }
}
