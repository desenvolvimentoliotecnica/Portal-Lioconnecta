using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PortalRH.Api.Contracts.Admin.Auth;
using PortalRH.Api.Contracts.Admin.Polls;
using PortalRH.Api.Contracts.Polls;
using PortalRH.Api.Data;
using PortalRH.Api.Domain;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

public class PollService : IPollService
{
    private readonly PortalRhDbContext _dbContext;

    public PollService(PortalRhDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PollDto>> GetPublishedAsync(Guid? portalUserId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var items = await _dbContext.Polls
            .AsNoTracking()
            .Include(item => item.Options)
                .ThenInclude(item => item.Votes)
            .Include(item => item.Votes)
            .Where(item =>
                item.Status == PollStatusCatalog.Published ||
                item.Status == PollStatusCatalog.Closed)
            .OrderByDescending(item => item.IsFeatured)
            .ThenByDescending(item => item.PublishedAtUtc ?? item.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return items.Select(item => MapToPublicDto(item, portalUserId, now)).ToList();
    }

    public async Task<PollDto?> GetPublishedBySlugAsync(string slug, Guid? portalUserId, CancellationToken cancellationToken)
    {
        var normalizedSlug = NormalizeSlug(slug);
        var now = DateTime.UtcNow;

        var item = await _dbContext.Polls
            .AsNoTracking()
            .Include(entity => entity.Options)
                .ThenInclude(option => option.Votes)
            .Include(entity => entity.Votes)
            .FirstOrDefaultAsync(entity =>
                entity.Slug == normalizedSlug &&
                (entity.Status == PollStatusCatalog.Published || entity.Status == PollStatusCatalog.Closed),
                cancellationToken);

        return item is null ? null : MapToPublicDto(item, portalUserId, now);
    }

    public async Task<PollDto?> SubmitVoteAsync(Guid pollId, Guid portalUserId, IReadOnlyCollection<Guid> optionIds, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var poll = await _dbContext.Polls
            .Include(item => item.Options)
                .ThenInclude(item => item.Votes)
            .Include(item => item.Votes)
            .FirstOrDefaultAsync(item => item.Id == pollId, cancellationToken);

        if (poll is null)
        {
            return null;
        }

        if (!IsVoteOpen(poll, now))
        {
            throw new InvalidOperationException("A enquete nao esta aberta para votacao.");
        }

        var normalizedOptionIds = optionIds
            .Where(item => item != Guid.Empty)
            .Distinct()
            .ToList();

        if (normalizedOptionIds.Count == 0)
        {
            throw new InvalidOperationException("Selecione ao menos uma opcao para votar.");
        }

        if (!poll.AllowMultipleChoices && normalizedOptionIds.Count > 1)
        {
            throw new InvalidOperationException("Esta enquete permite apenas uma opcao por voto.");
        }

        var existingVotes = poll.Votes
            .Where(item => item.PortalUserId == portalUserId)
            .ToList();

        if (existingVotes.Count > 0)
        {
            throw new InvalidOperationException("Seu voto ja foi registrado nesta enquete.");
        }

        var validOptions = poll.Options
            .Where(item => normalizedOptionIds.Contains(item.Id))
            .Select(item => item.Id)
            .ToHashSet();

        if (validOptions.Count != normalizedOptionIds.Count)
        {
            throw new InvalidOperationException("Uma ou mais opcoes selecionadas nao pertencem a esta enquete.");
        }

        foreach (var optionId in normalizedOptionIds)
        {
            _dbContext.PollVotes.Add(new PollVote
            {
                Id = Guid.NewGuid(),
                PollId = poll.Id,
                PollOptionId = optionId,
                PortalUserId = portalUserId,
                CreatedAtUtc = now
            });
        }

        poll.UpdatedAtUtc = now;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var refreshed = await _dbContext.Polls
            .AsNoTracking()
            .Include(item => item.Options)
                .ThenInclude(item => item.Votes)
            .Include(item => item.Votes)
            .FirstAsync(item => item.Id == pollId, cancellationToken);

        return MapToPublicDto(refreshed, portalUserId, now);
    }

    public async Task<PollAdminListResponse> GetAdminListAsync(CancellationToken cancellationToken)
    {
        var items = await _dbContext.Polls
            .AsNoTracking()
            .Include(item => item.Options)
                .ThenInclude(item => item.Votes)
            .Include(item => item.Votes)
            .OrderByDescending(item => item.UpdatedAtUtc)
            .ThenByDescending(item => item.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var adminItems = items.Select(MapToAdminDto).ToList();

        return new PollAdminListResponse(
            adminItems,
            new PollAdminSummaryDto(
                adminItems.Count,
                adminItems.Count(item => item.Status == PollStatusCatalog.Published),
                adminItems.Count(item => item.Status == PollStatusCatalog.Draft),
                adminItems.Count(item => item.Status == PollStatusCatalog.Closed),
                adminItems.Count(item => item.Status == PollStatusCatalog.Archived),
                adminItems.Sum(item => item.TotalVotes)));
    }

    public async Task<PollAdminDto?> GetAdminByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _dbContext.Polls
            .AsNoTracking()
            .Include(entity => entity.Options)
                .ThenInclude(option => option.Votes)
            .Include(entity => entity.Votes)
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        return item is null ? null : MapToAdminDto(item);
    }

    public async Task<PollAdminDto> CreateAsync(UpsertPollRequest request, AdminProfileDto actor, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var entity = new Poll
        {
            Id = Guid.NewGuid(),
            Slug = await GenerateUniqueSlugAsync(request.Title, null, cancellationToken),
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            Body = request.Body.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            AttachmentLabel = string.IsNullOrWhiteSpace(request.AttachmentLabel) ? null : request.AttachmentLabel.Trim(),
            AttachmentUrl = string.IsNullOrWhiteSpace(request.AttachmentUrl) ? null : request.AttachmentUrl.Trim(),
            Audience = request.Audience.Trim(),
            Status = PollStatusCatalog.Normalize(request.Status),
            AllowMultipleChoices = request.AllowMultipleChoices,
            ResultsVisibility = PollResultsVisibilityCatalog.Normalize(request.ResultsVisibility),
            IsFeatured = request.IsFeatured,
            PublishedAtUtc = request.PublishedAtUtc,
            ClosesAtUtc = request.ClosesAtUtc,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            Options = NormalizeOptions(request.Options)
        };

        _dbContext.Polls.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToAdminDto(entity);
    }

    public async Task<PollAdminDto?> UpdateAsync(Guid id, UpsertPollRequest request, AdminProfileDto actor, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Polls
            .Include(item => item.Options)
            .Include(item => item.Votes)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Slug = await GenerateUniqueSlugAsync(request.Title, id, cancellationToken);
        entity.Title = request.Title.Trim();
        entity.Summary = request.Summary.Trim();
        entity.Body = request.Body.Trim();
        entity.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
        entity.AttachmentLabel = string.IsNullOrWhiteSpace(request.AttachmentLabel) ? null : request.AttachmentLabel.Trim();
        entity.AttachmentUrl = string.IsNullOrWhiteSpace(request.AttachmentUrl) ? null : request.AttachmentUrl.Trim();
        entity.Audience = request.Audience.Trim();
        entity.Status = PollStatusCatalog.Normalize(request.Status);
        entity.AllowMultipleChoices = request.AllowMultipleChoices;
        entity.ResultsVisibility = PollResultsVisibilityCatalog.Normalize(request.ResultsVisibility);
        entity.IsFeatured = request.IsFeatured;
        entity.PublishedAtUtc = request.PublishedAtUtc;
        entity.ClosesAtUtc = request.ClosesAtUtc;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        var incomingOptions = request.Options
            .Select((option, index) => new
            {
                option.Id,
                Label = option.Label.Trim(),
                DisplayOrder = index + 1
            })
            .ToList();

        var removableOptions = entity.Options
            .Where(item => incomingOptions.All(option => option.Id != item.Id))
            .ToList();

        if (removableOptions.Any(option => option.Votes.Any()))
        {
            throw new InvalidOperationException("Nao e possivel remover opcoes que ja receberam votos.");
        }

        foreach (var removable in removableOptions)
        {
            entity.Options.Remove(removable);
            _dbContext.PollOptions.Remove(removable);
        }

        foreach (var option in incomingOptions)
        {
            var existing = option.Id.HasValue
                ? entity.Options.FirstOrDefault(item => item.Id == option.Id.Value)
                : null;

            if (existing is null)
            {
                entity.Options.Add(new PollOption
                {
                    Id = option.Id ?? Guid.NewGuid(),
                    PollId = entity.Id,
                    Label = option.Label,
                    DisplayOrder = option.DisplayOrder
                });
                continue;
            }

            existing.Label = option.Label;
            existing.DisplayOrder = option.DisplayOrder;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var refreshed = await _dbContext.Polls
            .AsNoTracking()
            .Include(item => item.Options)
                .ThenInclude(item => item.Votes)
            .Include(item => item.Votes)
            .FirstAsync(item => item.Id == id, cancellationToken);

        return MapToAdminDto(refreshed);
    }

    public async Task<PollAdminDto?> UpdateStatusAsync(Guid id, string status, AdminProfileDto actor, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Polls
            .Include(item => item.Options)
                .ThenInclude(item => item.Votes)
            .Include(item => item.Votes)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Status = PollStatusCatalog.Normalize(status);
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToAdminDto(entity);
    }

    private static List<PollOption> NormalizeOptions(IEnumerable<UpsertPollOptionRequest> options)
    {
        return options
            .Select((option, index) => new PollOption
            {
                Id = option.Id ?? Guid.NewGuid(),
                Label = option.Label.Trim(),
                DisplayOrder = index + 1
            })
            .ToList();
    }

    private static PollAdminDto MapToAdminDto(Poll item)
    {
        var totalVotes = item.Votes.Count;
        var uniqueVoters = item.Votes.Select(vote => vote.PortalUserId).Distinct().Count();

        return new PollAdminDto(
            item.Id,
            item.Slug,
            item.Title,
            item.Summary,
            item.Body,
            item.ImageUrl,
            item.AttachmentLabel,
            item.AttachmentUrl,
            item.Audience,
            item.Status,
            PollStatusCatalog.GetLabel(item.Status),
            item.AllowMultipleChoices,
            item.ResultsVisibility,
            PollResultsVisibilityCatalog.GetLabel(item.ResultsVisibility),
            item.IsFeatured,
            item.PublishedAtUtc,
            item.ClosesAtUtc,
            totalVotes,
            uniqueVoters,
            item.CreatedAtUtc,
            item.UpdatedAtUtc,
            item.Options
                .OrderBy(option => option.DisplayOrder)
                .Select(option => new PollAdminOptionDto(
                    option.Id,
                    option.Label,
                    option.DisplayOrder,
                    option.Votes.Count))
                .ToList());
    }

    private static PollDto MapToPublicDto(Poll item, Guid? portalUserId, DateTime now)
    {
        var userOptionIds = portalUserId.HasValue
            ? item.Votes
                .Where(vote => vote.PortalUserId == portalUserId.Value)
                .Select(vote => vote.PollOptionId)
                .ToHashSet()
            : [];

        var totalVotes = item.Votes.Count;
        var effectiveStatus = GetEffectiveStatus(item, now);
        var resultsVisible = item.ResultsVisibility switch
        {
            PollResultsVisibilityCatalog.Always => true,
            PollResultsVisibilityCatalog.AfterClose => effectiveStatus == PollStatusCatalog.Closed,
            _ => userOptionIds.Count > 0 || effectiveStatus == PollStatusCatalog.Closed
        };

        return new PollDto(
            item.Id,
            item.Slug,
            item.Title,
            item.Summary,
            item.Body,
            item.ImageUrl,
            item.AttachmentLabel,
            item.AttachmentUrl,
            item.Audience,
            effectiveStatus,
            PollStatusCatalog.GetLabel(effectiveStatus),
            item.AllowMultipleChoices,
            item.ResultsVisibility,
            PollResultsVisibilityCatalog.GetLabel(item.ResultsVisibility),
            item.IsFeatured,
            item.PublishedAtUtc,
            item.ClosesAtUtc,
            totalVotes,
            userOptionIds.Count > 0,
            resultsVisible,
            item.Options
                .OrderBy(option => option.DisplayOrder)
                .Select(option =>
                {
                    var votes = option.Votes.Count;
                    var percentage = totalVotes <= 0
                        ? 0
                        : Math.Round((votes * 100d) / totalVotes, 1, MidpointRounding.AwayFromZero);

                    return new PollOptionDto(
                        option.Id,
                        option.Label,
                        option.DisplayOrder,
                        resultsVisible ? votes : 0,
                        resultsVisible ? percentage : 0,
                        userOptionIds.Contains(option.Id));
                })
                .ToList());
    }

    private static bool IsVoteOpen(Poll item, DateTime now)
        => GetEffectiveStatus(item, now) == PollStatusCatalog.Published;

    private static string GetEffectiveStatus(Poll item, DateTime now)
    {
        var normalized = PollStatusCatalog.Normalize(item.Status);
        if (normalized == PollStatusCatalog.Published)
        {
            if (item.PublishedAtUtc.HasValue && item.PublishedAtUtc.Value > now)
            {
                return PollStatusCatalog.Draft;
            }

            if (item.ClosesAtUtc.HasValue && item.ClosesAtUtc.Value <= now)
            {
                return PollStatusCatalog.Closed;
            }
        }

        return normalized;
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, Guid? currentId, CancellationToken cancellationToken)
    {
        var baseSlug = NormalizeSlug(title);
        var slugSeed = string.IsNullOrWhiteSpace(baseSlug) ? $"enquete-{Guid.NewGuid():N}" : baseSlug;
        var candidate = slugSeed;
        var suffix = 1;

        while (await _dbContext.Polls.AnyAsync(
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
}
