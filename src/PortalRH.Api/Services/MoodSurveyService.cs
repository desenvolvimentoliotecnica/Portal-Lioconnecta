using Microsoft.EntityFrameworkCore;
using PortalRH.Api.Contracts.MoodSurvey;
using PortalRH.Api.Data;
using PortalRH.Api.Domain;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

public class MoodSurveyService : IMoodSurveyService
{
    private const string DefaultTitle = "Como voce esta se sentindo hoje?";
    private static readonly TimeZoneInfo SaoPauloTimeZone = ResolveSaoPauloTimeZone();

    private readonly PortalRhDbContext _dbContext;
    private readonly IMoodSurveyFeedbackService _feedbackService;

    public MoodSurveyService(PortalRhDbContext dbContext, IMoodSurveyFeedbackService feedbackService)
    {
        _dbContext = dbContext;
        _feedbackService = feedbackService;
    }

    public async Task<MoodSurveyTodayResponse> GetTodayAsync(Guid portalUserId, CancellationToken cancellationToken)
    {
        var surveyDate = GetCurrentSurveyDate();
        var userVote = await _dbContext.MoodSurveyVotes
            .AsNoTracking()
            .Include(item => item.FeedbackMessage)
            .FirstOrDefaultAsync(
                item => item.PortalUserId == portalUserId && item.SurveyDate == surveyDate,
                cancellationToken);

        var voteCounts = await LoadVoteCountsAsync(surveyDate, cancellationToken);
        var thankYouMessage = await ResolveThankYouMessageAsync(userVote, cancellationToken);
        return BuildResponse(surveyDate, userVote?.OptionKey, voteCounts, thankYouMessage);
    }

    public async Task<MoodSurveyTodayResponse> SubmitVoteAsync(
        Guid portalUserId,
        string optionKey,
        MoodSurveyAuditContext auditContext,
        CancellationToken cancellationToken)
    {
        var normalizedOptionKey = optionKey?.Trim() ?? string.Empty;
        if (!MoodSurveyOptionCatalog.IsValid(normalizedOptionKey))
        {
            throw new InvalidOperationException("Opcao de humor invalida.");
        }

        var surveyDate = GetCurrentSurveyDate();
        var existingVote = await _dbContext.MoodSurveyVotes
            .FirstOrDefaultAsync(
                item => item.PortalUserId == portalUserId && item.SurveyDate == surveyDate,
                cancellationToken);

        if (existingVote is not null)
        {
            throw new InvalidOperationException("Seu humor de hoje ja foi registrado.");
        }

        var now = DateTime.UtcNow;
        var feedbackPick = await _feedbackService.PickRandomAsync(normalizedOptionKey, cancellationToken);

        _dbContext.MoodSurveyVotes.Add(new MoodSurveyVote
        {
            Id = Guid.NewGuid(),
            PortalUserId = portalUserId,
            OptionKey = normalizedOptionKey,
            SurveyDate = surveyDate,
            FeedbackMessageId = feedbackPick?.Id,
            CreatedAtUtc = now,
            IpAddress = auditContext.IpAddress,
            Origin = auditContext.Origin
        });

        _dbContext.MoodSurveyAuditLogs.Add(new MoodSurveyAuditLog
        {
            Id = Guid.NewGuid(),
            PortalUserId = portalUserId,
            ActionType = MoodSurveyAuditActionTypes.VoteSubmitted,
            OptionKey = normalizedOptionKey,
            SurveyDate = surveyDate,
            ActorLogin = auditContext.ActorLogin,
            ActorDisplayName = auditContext.ActorDisplayName,
            IpAddress = auditContext.IpAddress,
            Origin = auditContext.Origin,
            UserAgent = auditContext.UserAgent,
            CreatedAtUtc = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        var voteCounts = await LoadVoteCountsAsync(surveyDate, cancellationToken);
        var thankYouMessage = feedbackPick?.Message
            ?? MoodSurveyOptionCatalog.Find(normalizedOptionKey)?.ThankYouMessage;
        return BuildResponse(surveyDate, normalizedOptionKey, voteCounts, thankYouMessage);
    }

    public async Task<MoodSurveyDashboardResponse> GetDashboardAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        string? department,
        CancellationToken cancellationToken)
    {
        var currentDate = GetCurrentSurveyDate();
        var resolvedEndDate = endDate ?? currentDate;
        var resolvedStartDate = startDate ?? resolvedEndDate.AddDays(-6);

        if (resolvedStartDate > resolvedEndDate)
        {
            (resolvedStartDate, resolvedEndDate) = (resolvedEndDate, resolvedStartDate);
        }

        var normalizedDepartment = string.IsNullOrWhiteSpace(department) ? null : department.Trim();

        var votes = await _dbContext.MoodSurveyVotes
            .AsNoTracking()
            .Include(item => item.PortalUser)
            .Where(item => item.SurveyDate >= resolvedStartDate && item.SurveyDate <= resolvedEndDate)
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(normalizedDepartment))
        {
            var departmentSearch = normalizedDepartment.ToLowerInvariant();
            votes = votes
                .Where(item =>
                    item.PortalUser?.Department is not null &&
                    string.Equals(item.PortalUser.Department, normalizedDepartment, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var activeUsers = await _dbContext.PortalUsers
            .AsNoTracking()
            .CountAsync(item => item.IsActive, cancellationToken);

        var totalVotes = votes.Count;
        var uniqueUsers = votes.Select(item => item.PortalUserId).Distinct().Count();
        var motivatedCount = votes.Count(item => item.OptionKey == MoodSurveyOptionCatalog.Motivated);
        var goodCount = votes.Count(item => item.OptionKey == MoodSurveyOptionCatalog.Good);
        var tiredCount = votes.Count(item => item.OptionKey == MoodSurveyOptionCatalog.Tired);
        var participationRate = activeUsers == 0
            ? 0m
            : Math.Round((decimal)uniqueUsers / activeUsers * 100m, 1);

        var options = BuildOptionDistribution(votes, totalVotes);
        var departments = BuildDepartmentBreakdown(votes);
        var dailyTrend = BuildDailyTrend(votes, resolvedStartDate, resolvedEndDate);
        var departmentOptions = votes
            .Select(item => item.PortalUser?.Department)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .GroupBy(item => item!, StringComparer.OrdinalIgnoreCase)
            .Select(group => new MoodSurveyDepartmentFilterOptionDto(group.Key, group.Key, group.Count()))
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new MoodSurveyDashboardResponse(
            resolvedStartDate,
            resolvedEndDate,
            normalizedDepartment,
            new MoodSurveyDashboardSummaryDto(
                totalVotes,
                uniqueUsers,
                activeUsers,
                motivatedCount,
                goodCount,
                tiredCount,
                participationRate),
            options,
            departments,
            dailyTrend,
            departmentOptions);
    }

    private static IReadOnlyList<MoodSurveyOptionDistributionDto> BuildOptionDistribution(
        IReadOnlyCollection<MoodSurveyVote> votes,
        int totalVotes)
    {
        return MoodSurveyOptionCatalog.Options
            .Select(option =>
            {
                var count = votes.Count(item => item.OptionKey == option.Key);
                return new MoodSurveyOptionDistributionDto(
                    option.Key,
                    option.Label,
                    option.Emoji,
                    count,
                    CalculatePercentage(count, totalVotes));
            })
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<MoodSurveyDepartmentBreakdownDto> BuildDepartmentBreakdown(
        IReadOnlyCollection<MoodSurveyVote> votes)
    {
        return votes
            .GroupBy(item => item.PortalUser?.Department ?? "Sem departamento", StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var departmentVotes = group.ToList();
                var total = departmentVotes.Count;
                var motivated = departmentVotes.Count(item => item.OptionKey == MoodSurveyOptionCatalog.Motivated);
                var good = departmentVotes.Count(item => item.OptionKey == MoodSurveyOptionCatalog.Good);
                var tired = departmentVotes.Count(item => item.OptionKey == MoodSurveyOptionCatalog.Tired);

                return new MoodSurveyDepartmentBreakdownDto(
                    group.Key,
                    total,
                    motivated,
                    good,
                    tired,
                    BuildOptionDistribution(departmentVotes, total));
            })
            .OrderByDescending(item => item.TotalVotes)
            .ThenBy(item => item.Department, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<MoodSurveyDailyTrendDto> BuildDailyTrend(
        IReadOnlyCollection<MoodSurveyVote> votes,
        DateOnly startDate,
        DateOnly endDate)
    {
        var result = new List<MoodSurveyDailyTrendDto>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dayVotes = votes.Where(item => item.SurveyDate == date).ToList();
            result.Add(new MoodSurveyDailyTrendDto(
                date,
                dayVotes.Count,
                dayVotes.Count(item => item.OptionKey == MoodSurveyOptionCatalog.Motivated),
                dayVotes.Count(item => item.OptionKey == MoodSurveyOptionCatalog.Good),
                dayVotes.Count(item => item.OptionKey == MoodSurveyOptionCatalog.Tired)));
        }

        return result;
    }

    private static decimal CalculatePercentage(int count, int total)
    {
        if (total == 0)
        {
            return 0m;
        }

        return Math.Round((decimal)count / total * 100m, 1);
    }

    private async Task<Dictionary<string, int>> LoadVoteCountsAsync(DateOnly surveyDate, CancellationToken cancellationToken)
    {
        return await _dbContext.MoodSurveyVotes
            .AsNoTracking()
            .Where(item => item.SurveyDate == surveyDate)
            .GroupBy(item => item.OptionKey)
            .Select(group => new { OptionKey = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.OptionKey, item => item.Count, cancellationToken);
    }

    private async Task<string?> ResolveThankYouMessageAsync(MoodSurveyVote? vote, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(vote?.OptionKey))
        {
            return null;
        }

        var pick = await _feedbackService.PickRandomAsync(vote.OptionKey, cancellationToken);
        return pick?.Message ?? MoodSurveyOptionCatalog.Find(vote.OptionKey)?.ThankYouMessage;
    }

    private static MoodSurveyTodayResponse BuildResponse(
        DateOnly surveyDate,
        string? selectedOptionKey,
        IReadOnlyDictionary<string, int> voteCounts,
        string? thankYouMessage = null)
    {
        var rankedOptions = MoodSurveyOptionCatalog.Options
            .Select(option => new
            {
                Option = option,
                VoteCount = voteCounts.GetValueOrDefault(option.Key)
            })
            .OrderByDescending(item => item.VoteCount)
            .ThenBy(item => item.Option.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var items = new List<MoodSurveyOptionDto>();
        for (var index = 0; index < rankedOptions.Count; index++)
        {
            var current = rankedOptions[index];
            items.Add(new MoodSurveyOptionDto(
                current.Option.Key,
                current.Option.Emoji,
                current.Option.Label,
                BuildRankLabel(index + 1, current.VoteCount),
                current.VoteCount));
        }

        var selectedOption = MoodSurveyOptionCatalog.Find(selectedOptionKey);

        return new MoodSurveyTodayResponse(
            DefaultTitle,
            surveyDate,
            selectedOption is not null,
            selectedOption?.Key,
            thankYouMessage ?? selectedOption?.ThankYouMessage,
            items);
    }

    private static string BuildRankLabel(int position, int voteCount)
    {
        if (voteCount == 0)
        {
            return "Aguardando votos";
        }

        return position switch
        {
            1 => "1º mais votado",
            2 => "2º mais votado",
            3 => "3º mais votado",
            _ => $"{position}º mais votado"
        };
    }

    private static DateOnly GetCurrentSurveyDate()
    {
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, SaoPauloTimeZone);
        return DateOnly.FromDateTime(nowLocal);
    }

    private static TimeZoneInfo ResolveSaoPauloTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
    }
}
