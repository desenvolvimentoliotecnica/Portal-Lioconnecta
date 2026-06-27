using Microsoft.EntityFrameworkCore;
using PortalRH.Api.Contracts.Communications;
using PortalRH.Api.Data;
using PortalRH.Api.Domain;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;
using System.Globalization;
using System.Text;

namespace PortalRH.Api.Services;

public class CommunicationService : ICommunicationService
{
    private readonly PortalRhDbContext _dbContext;

    public CommunicationService(PortalRhDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CommunicationDto>> GetAllAsync(Guid? portalUserId, CancellationToken cancellationToken)
    {
        var items = await _dbContext.Communications
            .AsNoTracking()
            .OrderByDescending(item => item.PublishedAt)
            .ThenByDescending(item => item.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var engagement = await LoadEngagementAsync(
            items.Select(item => item.Id).ToList(),
            portalUserId,
            cancellationToken);

        return items
            .Select(item => MapToDto(item, engagement))
            .ToList();
    }

    public async Task<CommunicationDto?> GetByIdAsync(Guid id, Guid? portalUserId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Communications
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var engagement = await LoadEngagementAsync([entity.Id], portalUserId, cancellationToken);
        return MapToDto(entity, engagement);
    }

    public async Task<CommunicationDto?> GetBySlugAsync(string slug, Guid? portalUserId, CancellationToken cancellationToken)
    {
        var normalizedSlug = NormalizeSlug(slug);

        var entity = await _dbContext.Communications
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Slug == normalizedSlug, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var engagement = await LoadEngagementAsync([entity.Id], portalUserId, cancellationToken);
        return MapToDto(entity, engagement);
    }

    public async Task<CommunicationDto> CreateAsync(UpsertCommunicationRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var slug = await GenerateUniqueSlugAsync(request.Title, null, cancellationToken);

        var entity = new Communication
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            Category = request.Category.Trim(),
            Priority = request.Priority.Trim(),
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            Body = request.Body.Trim(),
            Audience = request.Audience.Trim(),
            Channel = request.Channel.Trim(),
            Status = request.Status.Trim(),
            AttachmentLabel = request.AttachmentLabel.Trim(),
            Owner = request.Owner.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            IsFeatured = request.IsFeatured,
            PublishedAt = request.PublishedAt,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _dbContext.Communications.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(entity, EngagementSnapshot.Empty);
    }

    public async Task<CommunicationDto?> UpdateAsync(Guid id, UpsertCommunicationRequest request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Communications
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Slug = await GenerateUniqueSlugAsync(request.Title, id, cancellationToken);
        entity.Category = request.Category.Trim();
        entity.Priority = request.Priority.Trim();
        entity.Title = request.Title.Trim();
        entity.Summary = request.Summary.Trim();
        entity.Body = request.Body.Trim();
        entity.Audience = request.Audience.Trim();
        entity.Channel = request.Channel.Trim();
        entity.Status = request.Status.Trim();
        entity.AttachmentLabel = request.AttachmentLabel.Trim();
        entity.Owner = request.Owner.Trim();
        entity.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
        entity.IsFeatured = request.IsFeatured;
        entity.PublishedAt = request.PublishedAt;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var engagement = await LoadEngagementAsync([entity.Id], null, cancellationToken);
        return MapToDto(entity, engagement);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Communications
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _dbContext.Communications.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<CommunicationLikeResponse?> ToggleLikeAsync(
        Guid communicationId,
        Guid portalUserId,
        CommunicationAuditContext auditContext,
        CancellationToken cancellationToken)
    {
        var communication = await _dbContext.Communications
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == communicationId, cancellationToken);

        if (communication is null)
        {
            return null;
        }

        if (!IsLikeableStatus(communication.Status))
        {
            throw new InvalidOperationException("Somente comunicados publicados podem receber curtidas.");
        }

        var existingLike = await _dbContext.CommunicationLikes
            .FirstOrDefaultAsync(
                item => item.CommunicationId == communicationId && item.PortalUserId == portalUserId,
                cancellationToken);

        var now = DateTime.UtcNow;
        string actionType;
        bool hasLiked;

        if (existingLike is not null)
        {
            _dbContext.CommunicationLikes.Remove(existingLike);
            actionType = CommunicationInteractionAuditActionTypes.LikeRemoved;
            hasLiked = false;
        }
        else
        {
            _dbContext.CommunicationLikes.Add(new CommunicationLike
            {
                Id = Guid.NewGuid(),
                CommunicationId = communicationId,
                PortalUserId = portalUserId,
                CreatedAtUtc = now,
                IpAddress = auditContext.IpAddress,
                Origin = auditContext.Origin
            });
            actionType = CommunicationInteractionAuditActionTypes.LikeRegistered;
            hasLiked = true;
        }

        _dbContext.CommunicationInteractionAuditLogs.Add(new CommunicationInteractionAuditLog
        {
            Id = Guid.NewGuid(),
            CommunicationId = communicationId,
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

        var likeCount = await _dbContext.CommunicationLikes
            .AsNoTracking()
            .CountAsync(item => item.CommunicationId == communicationId, cancellationToken);

        return new CommunicationLikeResponse(communicationId, likeCount, hasLiked);
    }

    public async Task<CommunicationSaveResponse?> ToggleSaveAsync(
        Guid communicationId,
        Guid portalUserId,
        CommunicationAuditContext auditContext,
        CancellationToken cancellationToken)
    {
        var communication = await _dbContext.Communications
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == communicationId, cancellationToken);

        if (communication is null)
        {
            return null;
        }

        if (!IsLikeableStatus(communication.Status))
        {
            throw new InvalidOperationException("Somente comunicados publicados podem ser salvos.");
        }

        var existingSave = await _dbContext.CommunicationSaves
            .FirstOrDefaultAsync(
                item => item.CommunicationId == communicationId && item.PortalUserId == portalUserId,
                cancellationToken);

        var now = DateTime.UtcNow;
        string actionType;
        bool hasSaved;

        if (existingSave is not null)
        {
            _dbContext.CommunicationSaves.Remove(existingSave);
            actionType = CommunicationInteractionAuditActionTypes.SaveRemoved;
            hasSaved = false;
        }
        else
        {
            _dbContext.CommunicationSaves.Add(new CommunicationSave
            {
                Id = Guid.NewGuid(),
                CommunicationId = communicationId,
                PortalUserId = portalUserId,
                CreatedAtUtc = now,
                IpAddress = auditContext.IpAddress,
                Origin = auditContext.Origin
            });
            actionType = CommunicationInteractionAuditActionTypes.SaveRegistered;
            hasSaved = true;
        }

        _dbContext.CommunicationInteractionAuditLogs.Add(new CommunicationInteractionAuditLog
        {
            Id = Guid.NewGuid(),
            CommunicationId = communicationId,
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

        return new CommunicationSaveResponse(communicationId, hasSaved);
    }

    public async Task<CommunicationShareResponse?> ToggleShareAsync(
        Guid communicationId,
        Guid portalUserId,
        CommunicationAuditContext auditContext,
        CancellationToken cancellationToken)
    {
        var communication = await _dbContext.Communications
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == communicationId, cancellationToken);

        if (communication is null)
        {
            return null;
        }

        if (!IsLikeableStatus(communication.Status))
        {
            throw new InvalidOperationException("Somente comunicados publicados podem ser compartilhados.");
        }

        var existingShare = await _dbContext.CommunicationShares
            .FirstOrDefaultAsync(
                item => item.CommunicationId == communicationId && item.PortalUserId == portalUserId,
                cancellationToken);

        var now = DateTime.UtcNow;
        string actionType;
        bool hasShared;

        if (existingShare is not null)
        {
            _dbContext.CommunicationShares.Remove(existingShare);
            actionType = CommunicationInteractionAuditActionTypes.ShareRemoved;
            hasShared = false;
        }
        else
        {
            _dbContext.CommunicationShares.Add(new CommunicationShare
            {
                Id = Guid.NewGuid(),
                CommunicationId = communicationId,
                PortalUserId = portalUserId,
                CreatedAtUtc = now,
                IpAddress = auditContext.IpAddress,
                Origin = auditContext.Origin
            });
            actionType = CommunicationInteractionAuditActionTypes.ShareRegistered;
            hasShared = true;
        }

        _dbContext.CommunicationInteractionAuditLogs.Add(new CommunicationInteractionAuditLog
        {
            Id = Guid.NewGuid(),
            CommunicationId = communicationId,
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

        var shareCount = await _dbContext.CommunicationShares
            .AsNoTracking()
            .CountAsync(item => item.CommunicationId == communicationId, cancellationToken);

        return new CommunicationShareResponse(communicationId, shareCount, hasShared);
    }

    private async Task<EngagementSnapshot> LoadEngagementAsync(
        IReadOnlyCollection<Guid> communicationIds,
        Guid? portalUserId,
        CancellationToken cancellationToken)
    {
        if (communicationIds.Count == 0)
        {
            return EngagementSnapshot.Empty;
        }

        var likeCounts = await _dbContext.CommunicationLikes
            .AsNoTracking()
            .Where(item => communicationIds.Contains(item.CommunicationId))
            .GroupBy(item => item.CommunicationId)
            .Select(group => new { CommunicationId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.CommunicationId, item => item.Count, cancellationToken);

        HashSet<Guid> likedIds = [];
        HashSet<Guid> savedIds = [];
        if (portalUserId.HasValue)
        {
            var userLikes = await _dbContext.CommunicationLikes
                .AsNoTracking()
                .Where(item => item.PortalUserId == portalUserId.Value && communicationIds.Contains(item.CommunicationId))
                .Select(item => item.CommunicationId)
                .ToListAsync(cancellationToken);

            likedIds = userLikes.ToHashSet();

            var userSaves = await _dbContext.CommunicationSaves
                .AsNoTracking()
                .Where(item => item.PortalUserId == portalUserId.Value && communicationIds.Contains(item.CommunicationId))
                .Select(item => item.CommunicationId)
                .ToListAsync(cancellationToken);

            savedIds = userSaves.ToHashSet();
        }

        return new EngagementSnapshot(likeCounts, likedIds, savedIds);
    }

    private static CommunicationDto MapToDto(Communication item, EngagementSnapshot engagement)
    {
        engagement.LikeCounts.TryGetValue(item.Id, out var likeCount);
        var hasLiked = engagement.LikedCommunicationIds.Contains(item.Id);
        var hasSaved = engagement.SavedCommunicationIds.Contains(item.Id);

        return new CommunicationDto(
            item.Id,
            item.Slug,
            item.Category,
            item.Priority,
            item.Title,
            item.Summary,
            item.Body,
            item.Audience,
            item.Channel,
            item.Status,
            item.AttachmentLabel,
            item.Owner,
            item.ImageUrl,
            item.IsFeatured,
            item.PublishedAt,
            item.CreatedAtUtc,
            item.UpdatedAtUtc,
            likeCount,
            hasLiked,
            hasSaved);
    }

    private static bool IsLikeableStatus(string status)
    {
        return string.Equals(status?.Trim(), "Publicado", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, Guid? currentId, CancellationToken cancellationToken)
    {
        var baseSlug = NormalizeSlug(title);
        var slugSeed = string.IsNullOrWhiteSpace(baseSlug) ? $"comunicado-{Guid.NewGuid():N}" : baseSlug;
        var candidate = slugSeed;
        var suffix = 1;

        while (await _dbContext.Communications.AnyAsync(
                   item => item.Slug == candidate && (!currentId.HasValue || item.Id != currentId.Value),
                   cancellationToken))
        {
            candidate = $"{slugSeed}-{suffix++}";
        }

        return candidate;
    }

    private static string NormalizeSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var chars = normalized
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();

        var slug = new string(chars);

        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return slug.Trim('-');
    }

    private sealed record EngagementSnapshot(
        IReadOnlyDictionary<Guid, int> LikeCounts,
        HashSet<Guid> LikedCommunicationIds,
        HashSet<Guid> SavedCommunicationIds)
    {
        public static EngagementSnapshot Empty { get; } = new(new Dictionary<Guid, int>(), [], []);
    }
}
