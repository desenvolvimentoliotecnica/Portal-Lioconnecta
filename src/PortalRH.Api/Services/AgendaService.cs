using Microsoft.EntityFrameworkCore;
using PortalRH.Api.Contracts.Agenda;
using PortalRH.Api.Data;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;
using System.Security.Cryptography;
using System.Text;

namespace PortalRH.Api.Services;

public class AgendaService : IAgendaService
{
    private const int MaxUpcomingItems = 10;
    private static readonly TimeZoneInfo SaoPauloTimeZone = ResolveSaoPauloTimeZone();

    private readonly PortalRhDbContext _dbContext;
    private readonly IMicrosoftGraphCalendarService _microsoftGraphCalendarService;

    public AgendaService(
        PortalRhDbContext dbContext,
        IMicrosoftGraphCalendarService microsoftGraphCalendarService)
    {
        _dbContext = dbContext;
        _microsoftGraphCalendarService = microsoftGraphCalendarService;
    }

    public async Task<AgendaDayResponse> GetTodayAsync(Guid portalUserId, CancellationToken cancellationToken)
    {
        var portalUser = await _dbContext.PortalUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == portalUserId, cancellationToken);

        if (portalUser is null)
        {
            var nowLocal = GetNowLocal();
            return new AgendaDayResponse(DateOnly.FromDateTime(nowLocal), 0, Array.Empty<AgendaItemDto>());
        }

        var nowUtc = DateTime.UtcNow;
        var nowLocalDate = DateOnly.FromDateTime(GetNowLocal());
        var graphEvents = await _microsoftGraphCalendarService.GetUpcomingEventsAsync(
            portalUser,
            MaxUpcomingItems,
            cancellationToken);
        var databaseEvents = await LoadDatabaseEventsAsync(portalUserId, nowUtc, cancellationToken);

        var items = MergeUpcomingEvents(
            graphEvents.Select(MapGraphEventToDto).ToList(),
            databaseEvents.Select(MapDatabaseEventToDto).ToList(),
            nowLocalDate)
            .OrderBy(item => item.StartAtUtc)
            .Take(MaxUpcomingItems)
            .ToList();

        return new AgendaDayResponse(nowLocalDate, items.Count, items);
    }

    private async Task<List<AgendaEvent>> LoadDatabaseEventsAsync(
        Guid portalUserId,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        return await _dbContext.AgendaEvents
            .AsNoTracking()
            .Where(item =>
                item.IsActive &&
                (item.PortalUserId == null || item.PortalUserId == portalUserId) &&
                item.EndAtUtc > nowUtc)
            .OrderBy(item => item.StartAtUtc)
            .ThenBy(item => item.Title)
            .Take(MaxUpcomingItems)
            .ToListAsync(cancellationToken);
    }

    private static List<AgendaItemDto> MergeUpcomingEvents(
        IReadOnlyList<AgendaItemDto> graphItems,
        IReadOnlyList<AgendaItemDto> databaseItems,
        DateOnly referenceDate)
    {
        var merged = new List<AgendaItemDto>(graphItems.Count + databaseItems.Count);
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in graphItems.Concat(databaseItems).OrderBy(entry => entry.StartAtUtc))
        {
            var key = $"{item.StartAtUtc:O}|{item.Title}";
            if (!seenKeys.Add(key))
            {
                continue;
            }

            merged.Add(item with { TimeLabel = FormatTimeLabel(item.StartAtUtc, referenceDate) });
        }

        return merged;
    }

    private static AgendaItemDto MapDatabaseEventToDto(AgendaEvent item)
    {
        return new AgendaItemDto(
            item.Id,
            item.Title,
            item.Description,
            item.Location,
            string.Empty,
            item.Source,
            item.Audience,
            null,
            Array.Empty<AgendaParticipantDto>(),
            NormalizeUtc(item.StartAtUtc),
            NormalizeUtc(item.EndAtUtc));
    }

    private static AgendaItemDto MapGraphEventToDto(MicrosoftGraphCalendarEvent item)
    {
        return new AgendaItemDto(
            CreateStableGuid(item.Id),
            item.Title,
            item.Description,
            item.Location,
            string.Empty,
            "microsoft-365",
            "Usuario autenticado",
            item.JoinUrl,
            item.Participants,
            NormalizeUtc(item.StartAtUtc),
            NormalizeUtc(item.EndAtUtc));
    }

    private static string FormatTimeLabel(DateTime startAtUtc, DateOnly referenceDate)
    {
        var startLocal = TimeZoneInfo.ConvertTimeFromUtc(NormalizeUtc(startAtUtc), SaoPauloTimeZone);
        var startDate = DateOnly.FromDateTime(startLocal);
        return startDate == referenceDate
            ? startLocal.ToString("HH:mm")
            : startLocal.ToString("dd/MM HH:mm");
    }

    private static DateTime GetNowLocal()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, SaoPauloTimeZone);
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    private static Guid CreateStableGuid(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);
        return new Guid(guidBytes);
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
