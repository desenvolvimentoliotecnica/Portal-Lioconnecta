using Microsoft.EntityFrameworkCore;
using PortalRH.Api.Contracts.Feed;
using PortalRH.Api.Data;
using PortalRH.Api.Domain;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

public class FeedService : IFeedService
{
    private const string FeedTitle = "FEED LIOCONNECTA";
    private const string SavedFeedTitle = "ITENS SALVOS";
    private const int MaxPostLength = 2000;
    private const int MaxMediaPerPost = 10;
    private const int MaxMediaDescriptionLength = 500;

    private const int MaxMediaCommentLength = 1000;
    private const int MaxPostCommentLength = 2000;
    private const int MaxMentionsPerComment = 20;
    private const int MaxMentionsPerPost = 20;

    private static readonly HashSet<string> AllowedAspectRatios = new(StringComparer.OrdinalIgnoreCase)
    {
        "1:1", "16:9", "9:16", "free"
    };

    private readonly PortalRhDbContext _dbContext;
    private readonly ICommunicationService _communicationService;

    public FeedService(PortalRhDbContext dbContext, ICommunicationService communicationService)
    {
        _dbContext = dbContext;
        _communicationService = communicationService;
    }

    public async Task<FeedResponse> GetFeedAsync(Guid? portalUserId, CancellationToken cancellationToken)
    {
        var userPosts = await _dbContext.FeedPosts
            .AsNoTracking()
            .Include(item => item.PortalUser)
            .Include(item => item.Media)
            .Where(item => item.DeletedAtUtc == null)
            .OrderByDescending(item => item.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var communications = await _dbContext.Communications
            .AsNoTracking()
            .Where(item => item.Status.ToLower() == "publicado")
            .OrderByDescending(item => item.PublishedAt)
            .ThenByDescending(item => item.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var userPostIds = userPosts.Select(item => item.Id).ToList();
        var communicationIds = communications.Select(item => item.Id).ToList();

        var userPostLikeCounts = await LoadFeedPostLikeCountsAsync(userPostIds, cancellationToken);
        var userPostShareCounts = await LoadFeedPostShareCountsAsync(userPostIds, cancellationToken);
        var communicationLikeCounts = await LoadCommunicationLikeCountsAsync(communicationIds, cancellationToken);
        var communicationShareCounts = await LoadCommunicationShareCountsAsync(communicationIds, cancellationToken);
        var likedUserPostIds = portalUserId.HasValue
            ? await LoadLikedFeedPostIdsAsync(userPostIds, portalUserId.Value, cancellationToken)
            : [];
        var sharedUserPostIds = portalUserId.HasValue
            ? await LoadSharedFeedPostIdsAsync(userPostIds, portalUserId.Value, cancellationToken)
            : [];
        var likedCommunicationIds = portalUserId.HasValue
            ? await LoadLikedCommunicationIdsAsync(communicationIds, portalUserId.Value, cancellationToken)
            : [];
        var sharedCommunicationIds = portalUserId.HasValue
            ? await LoadSharedCommunicationIdsAsync(communicationIds, portalUserId.Value, cancellationToken)
            : [];
        var savedUserPostIds = portalUserId.HasValue
            ? await LoadSavedFeedPostIdsAsync(userPostIds, portalUserId.Value, cancellationToken)
            : [];
        var savedCommunicationIds = portalUserId.HasValue
            ? await LoadSavedCommunicationIdsAsync(communicationIds, portalUserId.Value, cancellationToken)
            : [];

        var mediaIds = userPosts.SelectMany(item => item.Media).Select(item => item.Id).ToList();
        var mediaCommentCounts = await LoadMediaCommentCountsAsync(mediaIds, cancellationToken);
        var postComments = await LoadFeedPostCommentsAsync(userPostIds, cancellationToken);
        var postMentions = await LoadFeedPostMentionsAsync(userPostIds, cancellationToken);

        var items = new List<FeedItemDto>();

        items.AddRange(userPosts.Select(item =>
        {
            var media = MapMediaItems(item.Media, mediaCommentCounts);
            var comments = postComments.GetValueOrDefault(item.Id) ?? [];
            var mentions = postMentions.GetValueOrDefault(item.Id) ?? [];
            return new FeedItemDto(
                item.Id,
                FeedItemSources.UserPost,
                null,
                item.PortalUser?.DisplayName ?? "Colaborador",
                item.PortalUserId,
                item.PortalUser?.Department ?? "Companhia",
                item.CreatedAtUtc,
                item.Text,
                mentions,
                null,
                null,
                media.FirstOrDefault()?.Url,
                userPostLikeCounts.GetValueOrDefault(item.Id),
                likedUserPostIds.Contains(item.Id),
                userPostShareCounts.GetValueOrDefault(item.Id),
                sharedUserPostIds.Contains(item.Id),
                savedUserPostIds.Contains(item.Id),
                media,
                comments.Count,
                comments);
        }));

        items.AddRange(communications.Select(item => new FeedItemDto(
            item.Id,
            FeedItemSources.Communication,
            item.Id,
            item.Owner,
            null,
            item.Category,
            item.PublishedAt,
            item.Summary,
            [],
            item.Title,
            item.Summary,
            item.ImageUrl,
            communicationLikeCounts.GetValueOrDefault(item.Id),
            likedCommunicationIds.Contains(item.Id),
            communicationShareCounts.GetValueOrDefault(item.Id),
            sharedCommunicationIds.Contains(item.Id),
            savedCommunicationIds.Contains(item.Id),
            [],
            0,
            [])));

        return new FeedResponse(
            FeedTitle,
            items
                .OrderByDescending(item => item.PublishedAtUtc)
                .ToList());
    }

    public async Task<FeedItemDto> CreatePostAsync(
        Guid portalUserId,
        string text,
        IReadOnlyList<CreateFeedPostMediaItem> media,
        IReadOnlyList<Guid> mentionedUserIds,
        FeedAuditContext auditContext,
        CancellationToken cancellationToken)
    {
        var normalizedText = text?.Trim() ?? string.Empty;
        var normalizedMedia = NormalizeMediaItems(media);

        if (string.IsNullOrWhiteSpace(normalizedText) && normalizedMedia.Count == 0)
        {
            throw new InvalidOperationException("Informe um texto ou ao menos uma foto para publicar no feed.");
        }

        if (normalizedText.Length > MaxPostLength)
        {
            throw new InvalidOperationException($"A publicacao pode ter no maximo {MaxPostLength} caracteres.");
        }

        if (normalizedMedia.Count > MaxMediaPerPost)
        {
            throw new InvalidOperationException($"A publicacao pode ter no maximo {MaxMediaPerPost} fotos.");
        }

        var mentionIds = await ResolveMentionedUserIdsAsync(mentionedUserIds, cancellationToken);
        if (mentionIds.Count > MaxMentionsPerPost)
        {
            throw new InvalidOperationException($"A publicacao pode mencionar no maximo {MaxMentionsPerPost} usuarios.");
        }

        var user = await _dbContext.PortalUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == portalUserId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("Usuario do portal nao encontrado.");
        }

        var now = DateTime.UtcNow;
        var entity = new FeedPost
        {
            Id = Guid.NewGuid(),
            PortalUserId = portalUserId,
            Text = normalizedText,
            CreatedAtUtc = now,
            IpAddress = auditContext.IpAddress,
            Origin = auditContext.Origin
        };

        foreach (var mentionId in mentionIds)
        {
            entity.Mentions.Add(new FeedPostMention
            {
                Id = Guid.NewGuid(),
                FeedPostId = entity.Id,
                MentionedPortalUserId = mentionId
            });
        }

        for (var index = 0; index < normalizedMedia.Count; index++)
        {
            var mediaItem = normalizedMedia[index];
            entity.Media.Add(new FeedPostMedia
            {
                Id = Guid.NewGuid(),
                FeedPostId = entity.Id,
                Url = mediaItem.Url,
                Description = mediaItem.Description,
                AspectRatio = mediaItem.AspectRatio,
                SortOrder = index,
                CreatedAtUtc = now
            });
        }

        _dbContext.FeedPosts.Add(entity);
        _dbContext.FeedPostAuditLogs.Add(new FeedPostAuditLog
        {
            Id = Guid.NewGuid(),
            FeedPostId = entity.Id,
            PortalUserId = portalUserId,
            ActionType = FeedPostAuditActionTypes.PostCreated,
            ActorLogin = auditContext.ActorLogin,
            ActorDisplayName = auditContext.ActorDisplayName,
            IpAddress = auditContext.IpAddress,
            Origin = auditContext.Origin,
            UserAgent = auditContext.UserAgent,
            CreatedAtUtc = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        var mappedMedia = MapMediaItems(entity.Media, new Dictionary<Guid, int>());
        var mentionedUsers = mentionIds.Count == 0
            ? []
            : await _dbContext.PortalUsers
                .AsNoTracking()
                .Where(item => mentionIds.Contains(item.Id))
                .ToDictionaryAsync(item => item.Id, item => item.DisplayName, cancellationToken);
        var mappedMentions = entity.Mentions
            .Select(item => MapPostMention(item, mentionedUsers))
            .ToList();

        return new FeedItemDto(
            entity.Id,
            FeedItemSources.UserPost,
            null,
            user.DisplayName,
            portalUserId,
            user.Department ?? "Companhia",
            entity.CreatedAtUtc,
            entity.Text,
            mappedMentions,
            null,
            null,
            mappedMedia.FirstOrDefault()?.Url,
            0,
            false,
            0,
            false,
            false,
            mappedMedia,
            0,
            []);
    }

    public async Task<FeedLikeResponse?> ToggleLikeAsync(
        Guid itemId,
        string source,
        Guid portalUserId,
        FeedAuditContext auditContext,
        CancellationToken cancellationToken)
    {
        var normalizedSource = source?.Trim() ?? string.Empty;

        if (string.Equals(normalizedSource, FeedItemSources.Communication, StringComparison.OrdinalIgnoreCase))
        {
            var communicationResult = await _communicationService.ToggleLikeAsync(
                itemId,
                portalUserId,
                MapAuditContext(auditContext),
                cancellationToken);

            return communicationResult is null
                ? null
                : new FeedLikeResponse(itemId, FeedItemSources.Communication, communicationResult.LikeCount, communicationResult.HasLiked);
        }

        if (string.Equals(normalizedSource, FeedItemSources.UserPost, StringComparison.OrdinalIgnoreCase))
        {
            return await ToggleUserPostLikeAsync(itemId, portalUserId, auditContext, cancellationToken);
        }

        throw new InvalidOperationException("Origem da publicacao invalida para curtida.");
    }

    public async Task<FeedShareResponse?> ToggleShareAsync(
        Guid itemId,
        string source,
        Guid portalUserId,
        FeedAuditContext auditContext,
        CancellationToken cancellationToken)
    {
        var normalizedSource = source?.Trim() ?? string.Empty;

        if (string.Equals(normalizedSource, FeedItemSources.Communication, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Comunicados nao podem ser compartilhados.");
        }

        if (string.Equals(normalizedSource, FeedItemSources.UserPost, StringComparison.OrdinalIgnoreCase))
        {
            return await ToggleUserPostShareAsync(itemId, portalUserId, auditContext, cancellationToken);
        }

        throw new InvalidOperationException("Origem da publicacao invalida para compartilhamento.");
    }

    public async Task<FeedSaveResponse?> ToggleSaveAsync(
        Guid itemId,
        string source,
        Guid portalUserId,
        FeedAuditContext auditContext,
        CancellationToken cancellationToken)
    {
        var normalizedSource = source?.Trim() ?? string.Empty;

        if (string.Equals(normalizedSource, FeedItemSources.Communication, StringComparison.OrdinalIgnoreCase))
        {
            var communicationResult = await _communicationService.ToggleSaveAsync(
                itemId,
                portalUserId,
                MapAuditContext(auditContext),
                cancellationToken);

            return communicationResult is null
                ? null
                : new FeedSaveResponse(itemId, FeedItemSources.Communication, communicationResult.HasSaved);
        }

        if (string.Equals(normalizedSource, FeedItemSources.UserPost, StringComparison.OrdinalIgnoreCase))
        {
            return await ToggleUserPostSaveAsync(itemId, portalUserId, auditContext, cancellationToken);
        }

        throw new InvalidOperationException("Origem da publicacao invalida para salvamento.");
    }

    public async Task<FeedResponse> GetSavedFeedAsync(Guid portalUserId, CancellationToken cancellationToken)
    {
        var postSaves = await _dbContext.FeedPostSaves
            .AsNoTracking()
            .Where(item => item.PortalUserId == portalUserId)
            .OrderByDescending(item => item.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var communicationSaves = await _dbContext.CommunicationSaves
            .AsNoTracking()
            .Where(item => item.PortalUserId == portalUserId)
            .OrderByDescending(item => item.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var userPostIds = postSaves.Select(item => item.FeedPostId).Distinct().ToList();
        var communicationIds = communicationSaves.Select(item => item.CommunicationId).Distinct().ToList();

        var userPosts = userPostIds.Count == 0
            ? []
            : await _dbContext.FeedPosts
                .AsNoTracking()
                .Include(item => item.PortalUser)
                .Include(item => item.Media)
                .Where(item => userPostIds.Contains(item.Id) && item.DeletedAtUtc == null)
                .ToListAsync(cancellationToken);

        var communications = communicationIds.Count == 0
            ? []
            : await _dbContext.Communications
                .AsNoTracking()
                .Where(item => communicationIds.Contains(item.Id))
                .Where(item => item.Status.ToLower() == "publicado")
                .ToListAsync(cancellationToken);

        var userPostLikeCounts = await LoadFeedPostLikeCountsAsync(userPosts.Select(item => item.Id).ToList(), cancellationToken);
        var userPostShareCounts = await LoadFeedPostShareCountsAsync(userPosts.Select(item => item.Id).ToList(), cancellationToken);
        var communicationLikeCounts = await LoadCommunicationLikeCountsAsync(communications.Select(item => item.Id).ToList(), cancellationToken);
        var communicationShareCounts = await LoadCommunicationShareCountsAsync(communications.Select(item => item.Id).ToList(), cancellationToken);
        var likedUserPostIds = await LoadLikedFeedPostIdsAsync(userPosts.Select(item => item.Id).ToList(), portalUserId, cancellationToken);
        var sharedUserPostIds = await LoadSharedFeedPostIdsAsync(userPosts.Select(item => item.Id).ToList(), portalUserId, cancellationToken);
        var likedCommunicationIds = await LoadLikedCommunicationIdsAsync(communications.Select(item => item.Id).ToList(), portalUserId, cancellationToken);
        var sharedCommunicationIds = await LoadSharedCommunicationIdsAsync(communications.Select(item => item.Id).ToList(), portalUserId, cancellationToken);

        var mediaIds = userPosts.SelectMany(item => item.Media).Select(item => item.Id).ToList();
        var mediaCommentCounts = await LoadMediaCommentCountsAsync(mediaIds, cancellationToken);
        var postComments = await LoadFeedPostCommentsAsync(userPosts.Select(item => item.Id).ToList(), cancellationToken);
        var postMentions = await LoadFeedPostMentionsAsync(userPosts.Select(item => item.Id).ToList(), cancellationToken);

        var userPostsById = userPosts.ToDictionary(item => item.Id);
        var communicationsById = communications.ToDictionary(item => item.Id);
        var items = new List<(DateTime SavedAtUtc, FeedItemDto Item)>();

        foreach (var save in postSaves)
        {
            if (!userPostsById.TryGetValue(save.FeedPostId, out var item))
            {
                continue;
            }

            var media = MapMediaItems(item.Media, mediaCommentCounts);
            var comments = postComments.GetValueOrDefault(item.Id) ?? [];
            var mentions = postMentions.GetValueOrDefault(item.Id) ?? [];
            items.Add((save.CreatedAtUtc, new FeedItemDto(
                item.Id,
                FeedItemSources.UserPost,
                null,
                item.PortalUser?.DisplayName ?? "Colaborador",
                item.PortalUserId,
                item.PortalUser?.Department ?? "Companhia",
                item.CreatedAtUtc,
                item.Text,
                mentions,
                null,
                null,
                media.FirstOrDefault()?.Url,
                userPostLikeCounts.GetValueOrDefault(item.Id),
                likedUserPostIds.Contains(item.Id),
                userPostShareCounts.GetValueOrDefault(item.Id),
                sharedUserPostIds.Contains(item.Id),
                true,
                media,
                comments.Count,
                comments)));
        }

        foreach (var save in communicationSaves)
        {
            if (!communicationsById.TryGetValue(save.CommunicationId, out var item))
            {
                continue;
            }

            items.Add((save.CreatedAtUtc, new FeedItemDto(
                item.Id,
                FeedItemSources.Communication,
                item.Id,
                item.Owner,
                null,
                item.Category,
                item.PublishedAt,
                item.Summary,
                [],
                item.Title,
                item.Summary,
                item.ImageUrl,
                communicationLikeCounts.GetValueOrDefault(item.Id),
                likedCommunicationIds.Contains(item.Id),
                communicationShareCounts.GetValueOrDefault(item.Id),
                sharedCommunicationIds.Contains(item.Id),
                true,
                [],
                0,
                [])));
        }

        return new FeedResponse(
            SavedFeedTitle,
            items
                .OrderByDescending(item => item.SavedAtUtc)
                .Select(item => item.Item)
                .ToList());
    }

    public async Task<int> GetSavedItemCountAsync(Guid portalUserId, CancellationToken cancellationToken)
    {
        var postCount = await _dbContext.FeedPostSaves
            .AsNoTracking()
            .CountAsync(item => item.PortalUserId == portalUserId, cancellationToken);

        var communicationCount = await _dbContext.CommunicationSaves
            .AsNoTracking()
            .CountAsync(item => item.PortalUserId == portalUserId, cancellationToken);

        return postCount + communicationCount;
    }

    public async Task<FeedMediaCommentsResponse?> GetMediaCommentsAsync(Guid mediaId, CancellationToken cancellationToken)
    {
        var media = await _dbContext.FeedPostMedia
            .AsNoTracking()
            .Include(item => item.FeedPost)
            .FirstOrDefaultAsync(item => item.Id == mediaId, cancellationToken);

        if (media is null || media.FeedPost?.DeletedAtUtc is not null)
        {
            return null;
        }

        var comments = await _dbContext.FeedPostMediaComments
            .AsNoTracking()
            .Include(item => item.PortalUser)
            .Where(item => item.FeedPostMediaId == mediaId)
            .OrderBy(item => item.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return new FeedMediaCommentsResponse(
            mediaId,
            comments.Select(item => MapMediaComment(item)).ToList());
    }

    public async Task<FeedMediaCommentDto?> CreateMediaCommentAsync(
        Guid mediaId,
        Guid portalUserId,
        string text,
        FeedAuditContext auditContext,
        CancellationToken cancellationToken)
    {
        var normalizedText = text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            throw new InvalidOperationException("Informe um comentario para a foto.");
        }

        if (normalizedText.Length > MaxMediaCommentLength)
        {
            throw new InvalidOperationException($"O comentario da foto pode ter no maximo {MaxMediaCommentLength} caracteres.");
        }

        var media = await _dbContext.FeedPostMedia
            .AsNoTracking()
            .Include(item => item.FeedPost)
            .FirstOrDefaultAsync(item => item.Id == mediaId, cancellationToken);

        if (media is null || media.FeedPost?.DeletedAtUtc is not null)
        {
            return null;
        }

        var user = await _dbContext.PortalUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == portalUserId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("Usuario do portal nao encontrado.");
        }

        var entity = new FeedPostMediaComment
        {
            Id = Guid.NewGuid(),
            FeedPostMediaId = mediaId,
            PortalUserId = portalUserId,
            Text = normalizedText,
            CreatedAtUtc = DateTime.UtcNow,
            IpAddress = auditContext.IpAddress,
            Origin = auditContext.Origin
        };

        _dbContext.FeedPostMediaComments.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapMediaComment(entity, user.DisplayName);
    }

    public async Task<FeedPostCommentsResponse?> GetPostCommentsAsync(Guid feedPostId, CancellationToken cancellationToken)
    {
        var feedPost = await FindActiveFeedPostAsync(feedPostId, cancellationToken);
        if (feedPost is null)
        {
            return null;
        }

        var comments = await LoadFeedPostCommentsAsync([feedPostId], cancellationToken);
        return new FeedPostCommentsResponse(feedPostId, comments.GetValueOrDefault(feedPostId) ?? []);
    }

    public async Task<FeedPostCommentDto?> CreatePostCommentAsync(
        Guid feedPostId,
        Guid portalUserId,
        string text,
        IReadOnlyList<Guid> mentionedUserIds,
        FeedAuditContext auditContext,
        CancellationToken cancellationToken)
    {
        var normalizedText = text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            throw new InvalidOperationException("Informe um comentario para publicar no post.");
        }

        if (normalizedText.Length > MaxPostCommentLength)
        {
            throw new InvalidOperationException($"O comentario pode ter no maximo {MaxPostCommentLength} caracteres.");
        }

        var feedPost = await FindActiveFeedPostAsync(feedPostId, cancellationToken);
        if (feedPost is null)
        {
            return null;
        }

        var user = await _dbContext.PortalUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == portalUserId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("Usuario do portal nao encontrado.");
        }

        var mentionIds = await ResolveMentionedUserIdsAsync(mentionedUserIds, cancellationToken);
        if (mentionIds.Count > MaxMentionsPerComment)
        {
            throw new InvalidOperationException($"O comentario pode mencionar no maximo {MaxMentionsPerComment} usuarios.");
        }

        var now = DateTime.UtcNow;
        var entity = new FeedPostComment
        {
            Id = Guid.NewGuid(),
            FeedPostId = feedPostId,
            PortalUserId = portalUserId,
            Text = normalizedText,
            CreatedAtUtc = now,
            IpAddress = auditContext.IpAddress,
            Origin = auditContext.Origin
        };

        foreach (var mentionId in mentionIds)
        {
            entity.Mentions.Add(new FeedPostCommentMention
            {
                Id = Guid.NewGuid(),
                FeedPostCommentId = entity.Id,
                MentionedPortalUserId = mentionId
            });
        }

        _dbContext.FeedPostComments.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var mentionedUsers = mentionIds.Count == 0
            ? []
            : await _dbContext.PortalUsers
                .AsNoTracking()
                .Where(item => mentionIds.Contains(item.Id))
                .ToDictionaryAsync(item => item.Id, item => item.DisplayName, cancellationToken);

        return MapPostComment(entity, user.DisplayName, mentionedUsers);
    }

    public async Task<FeedMentionSuggestionsResponse> SuggestMentionsAsync(string query, CancellationToken cancellationToken)
    {
        var normalizedQuery = query?.Trim() ?? string.Empty;
        if (normalizedQuery.Length < 1)
        {
            return new FeedMentionSuggestionsResponse([]);
        }

        var lowered = normalizedQuery.ToLowerInvariant();
        var users = await _dbContext.PortalUsers
            .AsNoTracking()
            .Where(item => item.IsActive)
            .Where(item =>
                item.DisplayName.ToLower().Contains(lowered) ||
                item.Login.ToLower().Contains(lowered) ||
                (item.Email != null && item.Email.ToLower().Contains(lowered)))
            .OrderBy(item => item.DisplayName)
            .Take(8)
            .ToListAsync(cancellationToken);

        return new FeedMentionSuggestionsResponse(users.Select(item => new FeedMentionSuggestionDto(
            item.Id,
            item.DisplayName,
            item.Department ?? "Companhia")).ToList());
    }

    public async Task<bool> DeletePostAsync(
        Guid feedPostId,
        Guid portalUserId,
        FeedAuditContext auditContext,
        CancellationToken cancellationToken)
    {
        var feedPost = await _dbContext.FeedPosts
            .FirstOrDefaultAsync(item => item.Id == feedPostId, cancellationToken);

        if (feedPost is null || feedPost.DeletedAtUtc is not null)
        {
            return false;
        }

        if (feedPost.PortalUserId != portalUserId)
        {
            throw new InvalidOperationException("Voce so pode remover suas proprias publicacoes.");
        }

        var now = DateTime.UtcNow;
        feedPost.DeletedAtUtc = now;

        _dbContext.FeedPostAuditLogs.Add(new FeedPostAuditLog
        {
            Id = Guid.NewGuid(),
            FeedPostId = feedPostId,
            PortalUserId = portalUserId,
            ActionType = FeedPostAuditActionTypes.PostDeleted,
            ActorLogin = auditContext.ActorLogin,
            ActorDisplayName = auditContext.ActorDisplayName,
            IpAddress = auditContext.IpAddress,
            Origin = auditContext.Origin,
            UserAgent = auditContext.UserAgent,
            CreatedAtUtc = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<FeedPost?> FindActiveFeedPostAsync(
        Guid feedPostId,
        CancellationToken cancellationToken,
        bool tracking = false)
    {
        var query = tracking
            ? _dbContext.FeedPosts.AsQueryable()
            : _dbContext.FeedPosts.AsNoTracking();

        return await query.FirstOrDefaultAsync(
            item => item.Id == feedPostId && item.DeletedAtUtc == null,
            cancellationToken);
    }

    private static List<CreateFeedPostMediaItem> NormalizeMediaItems(IReadOnlyList<CreateFeedPostMediaItem>? media)
    {
        if (media is null || media.Count == 0)
        {
            return [];
        }

        var normalized = new List<CreateFeedPostMediaItem>();

        foreach (var item in media)
        {
            var url = item.Url?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new InvalidOperationException("Informe a URL de cada foto anexada.");
            }

            if (!url.Contains("/uploads/feed/", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("URL de foto invalida para publicacao no feed.");
            }

            var description = item.Description?.Trim() ?? string.Empty;
            if (description.Length > MaxMediaDescriptionLength)
            {
                throw new InvalidOperationException($"A descricao da foto pode ter no maximo {MaxMediaDescriptionLength} caracteres.");
            }

            var aspectRatio = item.AspectRatio?.Trim() ?? "free";
            if (!AllowedAspectRatios.Contains(aspectRatio))
            {
                aspectRatio = "free";
            }

            normalized.Add(new CreateFeedPostMediaItem
            {
                Url = url,
                Description = description,
                AspectRatio = aspectRatio
            });
        }

        return normalized;
    }

    private static FeedPostCommentDto MapPostComment(
        FeedPostComment comment,
        string? authorOverride = null,
        IReadOnlyDictionary<Guid, string>? mentionedUsers = null)
    {
        var mentions = comment.Mentions
            .Select(item => new FeedPostCommentMentionDto(
                item.MentionedPortalUserId,
                mentionedUsers?.GetValueOrDefault(item.MentionedPortalUserId)
                    ?? item.MentionedPortalUser?.DisplayName
                    ?? "Colaborador"))
            .ToList();

        return new FeedPostCommentDto(
            comment.Id,
            authorOverride ?? comment.PortalUser?.DisplayName ?? "Colaborador",
            comment.Text,
            comment.CreatedAtUtc,
            mentions);
    }

    private async Task<Dictionary<Guid, List<FeedPostCommentMentionDto>>> LoadFeedPostMentionsAsync(
        IReadOnlyCollection<Guid> feedPostIds,
        CancellationToken cancellationToken)
    {
        if (feedPostIds.Count == 0)
        {
            return [];
        }

        var mentions = await _dbContext.FeedPostMentions
            .AsNoTracking()
            .Include(item => item.MentionedPortalUser)
            .Where(item => feedPostIds.Contains(item.FeedPostId))
            .ToListAsync(cancellationToken);

        return mentions
            .GroupBy(item => item.FeedPostId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item => MapPostMention(item)).ToList());
    }

    private static FeedPostCommentMentionDto MapPostMention(
        FeedPostMention mention,
        IReadOnlyDictionary<Guid, string>? mentionedUsers = null)
    {
        return new FeedPostCommentMentionDto(
            mention.MentionedPortalUserId,
            mentionedUsers?.GetValueOrDefault(mention.MentionedPortalUserId)
                ?? mention.MentionedPortalUser?.DisplayName
                ?? "Colaborador");
    }

    private async Task<Dictionary<Guid, List<FeedPostCommentDto>>> LoadFeedPostCommentsAsync(
        IReadOnlyCollection<Guid> feedPostIds,
        CancellationToken cancellationToken)
    {
        if (feedPostIds.Count == 0)
        {
            return [];
        }

        var comments = await _dbContext.FeedPostComments
            .AsNoTracking()
            .Include(item => item.PortalUser)
            .Include(item => item.Mentions)
            .ThenInclude(item => item.MentionedPortalUser)
            .Where(item => feedPostIds.Contains(item.FeedPostId))
            .OrderBy(item => item.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return comments
            .GroupBy(item => item.FeedPostId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item => MapPostComment(item)).ToList());
    }

    private async Task<List<Guid>> ResolveMentionedUserIdsAsync(
        IReadOnlyList<Guid>? mentionedUserIds,
        CancellationToken cancellationToken)
    {
        if (mentionedUserIds is null || mentionedUserIds.Count == 0)
        {
            return [];
        }

        var uniqueIds = mentionedUserIds
            .Where(item => item != Guid.Empty)
            .Distinct()
            .ToList();

        if (uniqueIds.Count == 0)
        {
            return [];
        }

        var activeIds = await _dbContext.PortalUsers
            .AsNoTracking()
            .Where(item => item.IsActive && uniqueIds.Contains(item.Id))
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);

        return activeIds;
    }

    private static FeedMediaCommentDto MapMediaComment(FeedPostMediaComment comment, string? authorOverride = null)
    {
        return new FeedMediaCommentDto(
            comment.Id,
            authorOverride ?? comment.PortalUser?.DisplayName ?? "Colaborador",
            comment.Text,
            comment.CreatedAtUtc);
    }

    private static IReadOnlyList<FeedMediaItemDto> MapMediaItems(
        IEnumerable<FeedPostMedia> media,
        IReadOnlyDictionary<Guid, int> commentCounts)
    {
        return media
            .OrderBy(item => item.SortOrder)
            .Select(item => new FeedMediaItemDto(
                item.Id,
                NormalizeMediaPublicUrl(item.Url),
                item.Description,
                item.AspectRatio,
                item.SortOrder,
                commentCounts.GetValueOrDefault(item.Id)))
            .ToList();
    }

    private static string NormalizeMediaPublicUrl(string url)
    {
        var value = url?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var marker = "/uploads/feed/";
        var index = value.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            return value[index..];
        }

        return value;
    }

    private async Task<Dictionary<Guid, int>> LoadMediaCommentCountsAsync(
        IReadOnlyCollection<Guid> mediaIds,
        CancellationToken cancellationToken)
    {
        if (mediaIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.FeedPostMediaComments
            .AsNoTracking()
            .Where(item => mediaIds.Contains(item.FeedPostMediaId))
            .GroupBy(item => item.FeedPostMediaId)
            .Select(group => new { FeedPostMediaId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.FeedPostMediaId, item => item.Count, cancellationToken);
    }

    private async Task<FeedLikeResponse?> ToggleUserPostLikeAsync(
        Guid feedPostId,
        Guid portalUserId,
        FeedAuditContext auditContext,
        CancellationToken cancellationToken)
    {
        var feedPost = await FindActiveFeedPostAsync(feedPostId, cancellationToken);

        if (feedPost is null)
        {
            return null;
        }

        var existingLike = await _dbContext.FeedPostLikes
            .FirstOrDefaultAsync(
                item => item.FeedPostId == feedPostId && item.PortalUserId == portalUserId,
                cancellationToken);

        var now = DateTime.UtcNow;
        string actionType;
        bool hasLiked;

        if (existingLike is not null)
        {
            _dbContext.FeedPostLikes.Remove(existingLike);
            actionType = FeedPostAuditActionTypes.LikeRemoved;
            hasLiked = false;
        }
        else
        {
            _dbContext.FeedPostLikes.Add(new FeedPostLike
            {
                Id = Guid.NewGuid(),
                FeedPostId = feedPostId,
                PortalUserId = portalUserId,
                CreatedAtUtc = now,
                IpAddress = auditContext.IpAddress,
                Origin = auditContext.Origin
            });
            actionType = FeedPostAuditActionTypes.LikeRegistered;
            hasLiked = true;
        }

        _dbContext.FeedPostAuditLogs.Add(new FeedPostAuditLog
        {
            Id = Guid.NewGuid(),
            FeedPostId = feedPostId,
            PortalUserId = portalUserId,
            ActionType = actionType,
            ActorLogin = auditContext.ActorLogin,
            ActorDisplayName = auditContext.ActorDisplayName,
            IpAddress = auditContext.IpAddress,
            Origin = auditContext.Origin,
            UserAgent = auditContext.UserAgent,
            CreatedAtUtc = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        var likeCount = await _dbContext.FeedPostLikes
            .AsNoTracking()
            .CountAsync(item => item.FeedPostId == feedPostId, cancellationToken);

        return new FeedLikeResponse(feedPostId, FeedItemSources.UserPost, likeCount, hasLiked);
    }

    private async Task<FeedShareResponse?> ToggleUserPostShareAsync(
        Guid feedPostId,
        Guid portalUserId,
        FeedAuditContext auditContext,
        CancellationToken cancellationToken)
    {
        var feedPost = await FindActiveFeedPostAsync(feedPostId, cancellationToken);

        if (feedPost is null)
        {
            return null;
        }

        if (feedPost.PortalUserId == portalUserId)
        {
            throw new InvalidOperationException("Voce nao pode compartilhar suas proprias publicacoes.");
        }

        var existingShare = await _dbContext.FeedPostShares
            .FirstOrDefaultAsync(
                item => item.FeedPostId == feedPostId && item.PortalUserId == portalUserId,
                cancellationToken);

        var now = DateTime.UtcNow;
        string actionType;
        bool hasShared;

        if (existingShare is not null)
        {
            _dbContext.FeedPostShares.Remove(existingShare);
            actionType = FeedPostAuditActionTypes.ShareRemoved;
            hasShared = false;
        }
        else
        {
            _dbContext.FeedPostShares.Add(new FeedPostShare
            {
                Id = Guid.NewGuid(),
                FeedPostId = feedPostId,
                PortalUserId = portalUserId,
                CreatedAtUtc = now,
                IpAddress = auditContext.IpAddress,
                Origin = auditContext.Origin
            });
            actionType = FeedPostAuditActionTypes.ShareRegistered;
            hasShared = true;
        }

        _dbContext.FeedPostAuditLogs.Add(new FeedPostAuditLog
        {
            Id = Guid.NewGuid(),
            FeedPostId = feedPostId,
            PortalUserId = portalUserId,
            ActionType = actionType,
            ActorLogin = auditContext.ActorLogin,
            ActorDisplayName = auditContext.ActorDisplayName,
            IpAddress = auditContext.IpAddress,
            Origin = auditContext.Origin,
            UserAgent = auditContext.UserAgent,
            CreatedAtUtc = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        var shareCount = await _dbContext.FeedPostShares
            .AsNoTracking()
            .CountAsync(item => item.FeedPostId == feedPostId, cancellationToken);

        return new FeedShareResponse(feedPostId, FeedItemSources.UserPost, shareCount, hasShared);
    }

    private async Task<FeedSaveResponse?> ToggleUserPostSaveAsync(
        Guid feedPostId,
        Guid portalUserId,
        FeedAuditContext auditContext,
        CancellationToken cancellationToken)
    {
        var feedPost = await FindActiveFeedPostAsync(feedPostId, cancellationToken);

        if (feedPost is null)
        {
            return null;
        }

        var existingSave = await _dbContext.FeedPostSaves
            .FirstOrDefaultAsync(
                item => item.FeedPostId == feedPostId && item.PortalUserId == portalUserId,
                cancellationToken);

        var now = DateTime.UtcNow;
        string actionType;
        bool hasSaved;

        if (existingSave is not null)
        {
            _dbContext.FeedPostSaves.Remove(existingSave);
            actionType = FeedPostAuditActionTypes.SaveRemoved;
            hasSaved = false;
        }
        else
        {
            _dbContext.FeedPostSaves.Add(new FeedPostSave
            {
                Id = Guid.NewGuid(),
                FeedPostId = feedPostId,
                PortalUserId = portalUserId,
                CreatedAtUtc = now,
                IpAddress = auditContext.IpAddress,
                Origin = auditContext.Origin
            });
            actionType = FeedPostAuditActionTypes.SaveRegistered;
            hasSaved = true;
        }

        _dbContext.FeedPostAuditLogs.Add(new FeedPostAuditLog
        {
            Id = Guid.NewGuid(),
            FeedPostId = feedPostId,
            PortalUserId = portalUserId,
            ActionType = actionType,
            ActorLogin = auditContext.ActorLogin,
            ActorDisplayName = auditContext.ActorDisplayName,
            IpAddress = auditContext.IpAddress,
            Origin = auditContext.Origin,
            UserAgent = auditContext.UserAgent,
            CreatedAtUtc = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new FeedSaveResponse(feedPostId, FeedItemSources.UserPost, hasSaved);
    }

    private static CommunicationAuditContext MapAuditContext(FeedAuditContext auditContext)
    {
        return new CommunicationAuditContext(
            auditContext.ActorLogin,
            auditContext.ActorDisplayName,
            auditContext.IpAddress,
            auditContext.Origin,
            auditContext.UserAgent);
    }

    private async Task<Dictionary<Guid, int>> LoadFeedPostLikeCountsAsync(
        IReadOnlyCollection<Guid> feedPostIds,
        CancellationToken cancellationToken)
    {
        if (feedPostIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.FeedPostLikes
            .AsNoTracking()
            .Where(item => feedPostIds.Contains(item.FeedPostId))
            .GroupBy(item => item.FeedPostId)
            .Select(group => new { FeedPostId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.FeedPostId, item => item.Count, cancellationToken);
    }

    private async Task<HashSet<Guid>> LoadLikedFeedPostIdsAsync(
        IReadOnlyCollection<Guid> feedPostIds,
        Guid portalUserId,
        CancellationToken cancellationToken)
    {
        if (feedPostIds.Count == 0)
        {
            return [];
        }

        var ids = await _dbContext.FeedPostLikes
            .AsNoTracking()
            .Where(item => item.PortalUserId == portalUserId && feedPostIds.Contains(item.FeedPostId))
            .Select(item => item.FeedPostId)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }

    private async Task<Dictionary<Guid, int>> LoadCommunicationLikeCountsAsync(
        IReadOnlyCollection<Guid> communicationIds,
        CancellationToken cancellationToken)
    {
        if (communicationIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.CommunicationLikes
            .AsNoTracking()
            .Where(item => communicationIds.Contains(item.CommunicationId))
            .GroupBy(item => item.CommunicationId)
            .Select(group => new { CommunicationId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.CommunicationId, item => item.Count, cancellationToken);
    }

    private async Task<HashSet<Guid>> LoadLikedCommunicationIdsAsync(
        IReadOnlyCollection<Guid> communicationIds,
        Guid portalUserId,
        CancellationToken cancellationToken)
    {
        if (communicationIds.Count == 0)
        {
            return [];
        }

        var ids = await _dbContext.CommunicationLikes
            .AsNoTracking()
            .Where(item => item.PortalUserId == portalUserId && communicationIds.Contains(item.CommunicationId))
            .Select(item => item.CommunicationId)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }

    private async Task<Dictionary<Guid, int>> LoadFeedPostShareCountsAsync(
        IReadOnlyCollection<Guid> feedPostIds,
        CancellationToken cancellationToken)
    {
        if (feedPostIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.FeedPostShares
            .AsNoTracking()
            .Where(item => feedPostIds.Contains(item.FeedPostId))
            .GroupBy(item => item.FeedPostId)
            .Select(group => new { FeedPostId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.FeedPostId, item => item.Count, cancellationToken);
    }

    private async Task<HashSet<Guid>> LoadSharedFeedPostIdsAsync(
        IReadOnlyCollection<Guid> feedPostIds,
        Guid portalUserId,
        CancellationToken cancellationToken)
    {
        if (feedPostIds.Count == 0)
        {
            return [];
        }

        var ids = await _dbContext.FeedPostShares
            .AsNoTracking()
            .Where(item => item.PortalUserId == portalUserId && feedPostIds.Contains(item.FeedPostId))
            .Select(item => item.FeedPostId)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }

    private async Task<Dictionary<Guid, int>> LoadCommunicationShareCountsAsync(
        IReadOnlyCollection<Guid> communicationIds,
        CancellationToken cancellationToken)
    {
        if (communicationIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.CommunicationShares
            .AsNoTracking()
            .Where(item => communicationIds.Contains(item.CommunicationId))
            .GroupBy(item => item.CommunicationId)
            .Select(group => new { CommunicationId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.CommunicationId, item => item.Count, cancellationToken);
    }

    private async Task<HashSet<Guid>> LoadSharedCommunicationIdsAsync(
        IReadOnlyCollection<Guid> communicationIds,
        Guid portalUserId,
        CancellationToken cancellationToken)
    {
        if (communicationIds.Count == 0)
        {
            return [];
        }

        var ids = await _dbContext.CommunicationShares
            .AsNoTracking()
            .Where(item => item.PortalUserId == portalUserId && communicationIds.Contains(item.CommunicationId))
            .Select(item => item.CommunicationId)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }

    private async Task<HashSet<Guid>> LoadSavedFeedPostIdsAsync(
        IReadOnlyCollection<Guid> feedPostIds,
        Guid portalUserId,
        CancellationToken cancellationToken)
    {
        if (feedPostIds.Count == 0)
        {
            return [];
        }

        var ids = await _dbContext.FeedPostSaves
            .AsNoTracking()
            .Where(item => item.PortalUserId == portalUserId && feedPostIds.Contains(item.FeedPostId))
            .Select(item => item.FeedPostId)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }

    private async Task<HashSet<Guid>> LoadSavedCommunicationIdsAsync(
        IReadOnlyCollection<Guid> communicationIds,
        Guid portalUserId,
        CancellationToken cancellationToken)
    {
        if (communicationIds.Count == 0)
        {
            return [];
        }

        var ids = await _dbContext.CommunicationSaves
            .AsNoTracking()
            .Where(item => item.PortalUserId == portalUserId && communicationIds.Contains(item.CommunicationId))
            .Select(item => item.CommunicationId)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }
}
