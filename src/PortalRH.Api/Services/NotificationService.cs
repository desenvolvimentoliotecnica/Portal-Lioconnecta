using Microsoft.EntityFrameworkCore;
using PortalRH.Api.Contracts.Notifications;
using PortalRH.Api.Data;
using PortalRH.Api.Domain;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

public class NotificationService : INotificationService
{
    private const string CommunicationSourceType = "communication";
    private const string PollSourceType = "poll";

    private readonly PortalRhDbContext _dbContext;

    public NotificationService(PortalRhDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NotificationListResponse> GetForUserAsync(Guid portalUserId, CancellationToken cancellationToken)
    {
        await SyncSourceNotificationsAsync(cancellationToken);

        var reads = await _dbContext.PortalUserNotificationReads
            .AsNoTracking()
            .Where(item => item.PortalUserId == portalUserId)
            .ToDictionaryAsync(item => item.NotificationId, item => item.ReadAtUtc, cancellationToken);

        var items = await _dbContext.Notifications
            .AsNoTracking()
            .Where(item => item.IsActive)
            .OrderByDescending(item => item.PublishedAtUtc)
            .ThenByDescending(item => item.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var dtoItems = items.Select(item => MapToDto(item, reads)).ToList();
        var categoryCounts = dtoItems
            .Where(item => !item.IsRead)
            .GroupBy(item => item.Category)
            .OrderBy(item => item.Key)
            .ToDictionary(item => item.Key, item => item.Count(), StringComparer.OrdinalIgnoreCase);

        var unreadCount = dtoItems.Count(item => !item.IsRead);

        return new NotificationListResponse(
            dtoItems,
            new NotificationSummaryDto(
                dtoItems.Count,
                unreadCount,
                dtoItems.Count - unreadCount,
                categoryCounts));
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid portalUserId, CancellationToken cancellationToken)
    {
        await SyncSourceNotificationsAsync(cancellationToken);

        var exists = await _dbContext.Notifications
            .AnyAsync(item => item.Id == notificationId && item.IsActive, cancellationToken);

        if (!exists)
        {
            return false;
        }

        var alreadyRead = await _dbContext.PortalUserNotificationReads
            .AnyAsync(item => item.NotificationId == notificationId && item.PortalUserId == portalUserId, cancellationToken);

        if (!alreadyRead)
        {
            _dbContext.PortalUserNotificationReads.Add(new PortalUserNotificationRead
            {
                Id = Guid.NewGuid(),
                NotificationId = notificationId,
                PortalUserId = portalUserId,
                ReadAtUtc = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<int> MarkAllAsReadAsync(Guid portalUserId, CancellationToken cancellationToken)
    {
        await SyncSourceNotificationsAsync(cancellationToken);

        var activeNotificationIds = await _dbContext.Notifications
            .AsNoTracking()
            .Where(item => item.IsActive)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);

        var readNotificationIds = await _dbContext.PortalUserNotificationReads
            .AsNoTracking()
            .Where(item => item.PortalUserId == portalUserId)
            .Select(item => item.NotificationId)
            .ToListAsync(cancellationToken);

        var pendingIds = activeNotificationIds.Except(readNotificationIds).ToList();
        var now = DateTime.UtcNow;

        foreach (var notificationId in pendingIds)
        {
            _dbContext.PortalUserNotificationReads.Add(new PortalUserNotificationRead
            {
                Id = Guid.NewGuid(),
                NotificationId = notificationId,
                PortalUserId = portalUserId,
                ReadAtUtc = now
            });
        }

        if (pendingIds.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return pendingIds.Count;
    }

    private async Task SyncSourceNotificationsAsync(CancellationToken cancellationToken)
    {
        var existing = await _dbContext.Notifications
            .ToDictionaryAsync(item => $"{item.SourceType}:{item.SourceId}", cancellationToken);

        var now = DateTime.UtcNow;
        var sourceKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var communications = await _dbContext.Communications
            .AsNoTracking()
            .Where(item => item.Status != "Rascunho" && item.Status != "Draft" && item.Status != "Arquivado")
            .ToListAsync(cancellationToken);

        foreach (var communication in communications)
        {
            var key = $"{CommunicationSourceType}:{communication.Id}";
            sourceKeys.Add(key);

            var publishedAtUtc = NormalizeUtc(communication.PublishedAt);
            var targetUrl = $"#comunicacao/leitura/{communication.Slug}";
            var notification = existing.GetValueOrDefault(key);

            if (notification is null)
            {
                _dbContext.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    SourceType = CommunicationSourceType,
                    SourceId = communication.Id,
                    Category = string.IsNullOrWhiteSpace(communication.Category) ? "Comunicados" : communication.Category,
                    Title = communication.Title,
                    Message = communication.Summary,
                    Tone = communication.Priority.Contains("alta", StringComparison.OrdinalIgnoreCase) ? "warning" : "info",
                    Icon = "fa-solid fa-bullhorn",
                    TargetUrl = targetUrl,
                    Audience = communication.Audience,
                    IsActive = true,
                    PublishedAtUtc = publishedAtUtc,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
                continue;
            }

            notification.Category = string.IsNullOrWhiteSpace(communication.Category) ? "Comunicados" : communication.Category;
            notification.Title = communication.Title;
            notification.Message = communication.Summary;
            notification.Tone = communication.Priority.Contains("alta", StringComparison.OrdinalIgnoreCase) ? "warning" : "info";
            notification.Icon = "fa-solid fa-bullhorn";
            notification.TargetUrl = targetUrl;
            notification.Audience = communication.Audience;
            notification.IsActive = true;
            notification.PublishedAtUtc = publishedAtUtc;
            notification.UpdatedAtUtc = now;
        }

        var polls = await _dbContext.Polls
            .AsNoTracking()
            .Where(item => item.Status == PollStatusCatalog.Published || item.Status == PollStatusCatalog.Closed)
            .ToListAsync(cancellationToken);

        foreach (var poll in polls)
        {
            var key = $"{PollSourceType}:{poll.Id}";
            sourceKeys.Add(key);

            var publishedAtUtc = poll.PublishedAtUtc ?? poll.CreatedAtUtc;
            var targetUrl = $"#enquetes/leitura/{poll.Slug}";
            var notification = existing.GetValueOrDefault(key);

            if (notification is null)
            {
                _dbContext.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    SourceType = PollSourceType,
                    SourceId = poll.Id,
                    Category = "Enquetes",
                    Title = poll.Title,
                    Message = poll.Summary,
                    Tone = poll.Status == PollStatusCatalog.Closed ? "neutral" : "success",
                    Icon = "fa-solid fa-square-poll-vertical",
                    TargetUrl = targetUrl,
                    Audience = poll.Audience,
                    IsActive = true,
                    PublishedAtUtc = NormalizeUtc(publishedAtUtc),
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
                continue;
            }

            notification.Category = "Enquetes";
            notification.Title = poll.Title;
            notification.Message = poll.Summary;
            notification.Tone = poll.Status == PollStatusCatalog.Closed ? "neutral" : "success";
            notification.Icon = "fa-solid fa-square-poll-vertical";
            notification.TargetUrl = targetUrl;
            notification.Audience = poll.Audience;
            notification.IsActive = true;
            notification.PublishedAtUtc = NormalizeUtc(publishedAtUtc);
            notification.UpdatedAtUtc = now;
        }

        foreach (var notification in existing.Values.Where(item => !sourceKeys.Contains($"{item.SourceType}:{item.SourceId}")))
        {
            notification.IsActive = false;
            notification.UpdatedAtUtc = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static NotificationDto MapToDto(Notification item, IReadOnlyDictionary<Guid, DateTime> reads)
    {
        var isRead = reads.TryGetValue(item.Id, out var readAtUtc);

        return new NotificationDto(
            item.Id,
            item.Category,
            item.Title,
            item.Message,
            item.Tone,
            item.Icon,
            item.TargetUrl,
            item.Audience,
            item.SourceType,
            item.SourceId,
            item.PublishedAtUtc,
            isRead,
            isRead ? readAtUtc : null);
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc)
        {
            return value;
        }

        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
