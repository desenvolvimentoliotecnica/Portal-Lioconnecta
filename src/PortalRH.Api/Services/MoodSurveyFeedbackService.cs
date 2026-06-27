using Microsoft.EntityFrameworkCore;
using PortalRH.Api.Contracts.MoodSurvey;
using PortalRH.Api.Data;
using PortalRH.Api.Domain;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

public class MoodSurveyFeedbackService : IMoodSurveyFeedbackService
{
    private readonly PortalRhDbContext _dbContext;

    public MoodSurveyFeedbackService(PortalRhDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureSeedAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.MoodSurveyFeedbackMessages.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var seedMessages = MoodSurveyFeedbackSeedData.BuildMessages()
            .Select(item => new MoodSurveyFeedbackMessage
            {
                Id = item.Id,
                OptionKey = item.OptionKey,
                Message = item.Message,
                SortOrder = item.SortOrder,
                IsActive = item.IsActive,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            })
            .ToList();

        _dbContext.MoodSurveyFeedbackMessages.AddRange(seedMessages);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MoodSurveyFeedbackMessageListResponse> GetAllAsync(string? optionKey, CancellationToken cancellationToken)
    {
        var normalizedOptionKey = NormalizeOptionKey(optionKey);
        var query = _dbContext.MoodSurveyFeedbackMessages.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(normalizedOptionKey))
        {
            query = query.Where(item => item.OptionKey == normalizedOptionKey);
        }

        var items = await query
            .OrderBy(item => item.OptionKey)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Message)
            .ToListAsync(cancellationToken);

        return new MoodSurveyFeedbackMessageListResponse(
            items.Select(MapToDto).ToList(),
            BuildOptionSummaries(items));
    }

    public async Task<MoodSurveyFeedbackMessageDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _dbContext.MoodSurveyFeedbackMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        return item is null ? null : MapToDto(item);
    }

    public async Task<MoodSurveyFeedbackMessageDto> CreateAsync(
        UpsertMoodSurveyFeedbackMessageRequest request,
        CancellationToken cancellationToken)
    {
        var optionKey = NormalizeAndValidateOptionKey(request.OptionKey);
        var message = NormalizeMessage(request.Message);
        var now = DateTime.UtcNow;
        var sortOrder = request.SortOrder ?? await ResolveNextSortOrderAsync(optionKey, cancellationToken);

        var entity = new MoodSurveyFeedbackMessage
        {
            Id = Guid.NewGuid(),
            OptionKey = optionKey,
            Message = message,
            SortOrder = sortOrder,
            IsActive = request.IsActive,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _dbContext.MoodSurveyFeedbackMessages.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(entity);
    }

    public async Task<MoodSurveyFeedbackMessageDto?> UpdateAsync(
        Guid id,
        UpsertMoodSurveyFeedbackMessageRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await _dbContext.MoodSurveyFeedbackMessages
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var optionKey = NormalizeAndValidateOptionKey(request.OptionKey);
        entity.OptionKey = optionKey;
        entity.Message = NormalizeMessage(request.Message);
        entity.SortOrder = request.SortOrder ?? entity.SortOrder;
        entity.IsActive = request.IsActive;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.MoodSurveyFeedbackMessages
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        var activeCount = await _dbContext.MoodSurveyFeedbackMessages
            .CountAsync(
                item => item.OptionKey == entity.OptionKey && item.IsActive && item.Id != entity.Id,
                cancellationToken);

        if (entity.IsActive && activeCount == 0)
        {
            throw new InvalidOperationException("Mantenha pelo menos uma mensagem ativa para este humor.");
        }

        _dbContext.MoodSurveyFeedbackMessages.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<MoodSurveyFeedbackPickResult?> PickRandomAsync(string optionKey, CancellationToken cancellationToken)
    {
        var message = await PickRandomActiveAsync(optionKey, cancellationToken);
        return message is null ? null : new MoodSurveyFeedbackPickResult(message.Id, message.Message);
    }

    private async Task<MoodSurveyFeedbackMessage?> PickRandomActiveAsync(string optionKey, CancellationToken cancellationToken)
    {
        var normalizedOptionKey = NormalizeAndValidateOptionKey(optionKey);
        var messages = await _dbContext.MoodSurveyFeedbackMessages
            .AsNoTracking()
            .Where(item => item.OptionKey == normalizedOptionKey && item.IsActive)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return null;
        }

        var index = Random.Shared.Next(messages.Count);
        return messages[index];
    }

    private async Task<int> ResolveNextSortOrderAsync(string optionKey, CancellationToken cancellationToken)
    {
        var maxSortOrder = await _dbContext.MoodSurveyFeedbackMessages
            .Where(item => item.OptionKey == optionKey)
            .Select(item => (int?)item.SortOrder)
            .MaxAsync(cancellationToken);

        return (maxSortOrder ?? 0) + 1;
    }

    private static string? NormalizeOptionKey(string? optionKey)
    {
        return string.IsNullOrWhiteSpace(optionKey) ? null : optionKey.Trim();
    }

    private static string NormalizeAndValidateOptionKey(string? optionKey)
    {
        var normalized = NormalizeOptionKey(optionKey) ?? string.Empty;
        if (!MoodSurveyOptionCatalog.IsValid(normalized))
        {
            throw new InvalidOperationException("Opcao de humor invalida.");
        }

        return normalized;
    }

    private static string NormalizeMessage(string? message)
    {
        var normalized = message?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Informe o texto da mensagem de feedback.");
        }

        return normalized;
    }

    private static MoodSurveyFeedbackMessageDto MapToDto(MoodSurveyFeedbackMessage item)
    {
        var option = MoodSurveyOptionCatalog.Find(item.OptionKey);

        return new MoodSurveyFeedbackMessageDto(
            item.Id,
            item.OptionKey,
            option?.Label ?? item.OptionKey,
            option?.Emoji ?? "🙂",
            item.Message,
            item.SortOrder,
            item.IsActive,
            item.CreatedAtUtc,
            item.UpdatedAtUtc);
    }

    private static IReadOnlyList<MoodSurveyFeedbackOptionSummaryDto> BuildOptionSummaries(
        IReadOnlyCollection<MoodSurveyFeedbackMessage> items)
    {
        return MoodSurveyOptionCatalog.Options
            .Select(option =>
            {
                var optionMessages = items.Where(item => item.OptionKey == option.Key).ToList();
                return new MoodSurveyFeedbackOptionSummaryDto(
                    option.Key,
                    option.Label,
                    option.Emoji,
                    optionMessages.Count,
                    optionMessages.Count(item => item.IsActive));
            })
            .ToList();
    }
}
