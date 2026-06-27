using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PortalRH.Api.Contracts.Agenda;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

public class MicrosoftGraphCalendarService : IMicrosoftGraphCalendarService
{
    private const int DefaultLookaheadDays = 120;

    private static readonly TimeZoneInfo SaoPauloTimeZone = ResolveSaoPauloTimeZone();

    private readonly IMicrosoftGraphConfigurationService _configurationService;
    private readonly MicrosoftGraphAuthClient _authClient;
    private readonly IMicrosoftGraphUserPhotoService _userPhotoService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MicrosoftGraphCalendarService> _logger;

    public MicrosoftGraphCalendarService(
        IMicrosoftGraphConfigurationService configurationService,
        MicrosoftGraphAuthClient authClient,
        IMicrosoftGraphUserPhotoService userPhotoService,
        IHttpClientFactory httpClientFactory,
        ILogger<MicrosoftGraphCalendarService> logger)
    {
        _configurationService = configurationService;
        _authClient = authClient;
        _userPhotoService = userPhotoService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MicrosoftGraphCalendarEvent>> GetUpcomingEventsAsync(
        PortalUser user,
        int limit,
        CancellationToken cancellationToken)
    {
        if (limit <= 0)
        {
            return Array.Empty<MicrosoftGraphCalendarEvent>();
        }

        var configuration = await _configurationService.GetRuntimeConfigurationAsync(cancellationToken);
        if (!configuration.IsEnabled)
        {
            return Array.Empty<MicrosoftGraphCalendarEvent>();
        }

        var userIdentifier = ResolveUserIdentifier(user, configuration.UserIdentifier);
        if (string.IsNullOrWhiteSpace(userIdentifier))
        {
            _logger.LogWarning(
                "Integracao Microsoft Graph habilitada, mas o usuario {PortalUserId} nao possui identificador para consulta.",
                user.Id);
            return Array.Empty<MicrosoftGraphCalendarEvent>();
        }

        var tokenResult = await _authClient.RequestAccessTokenAsync(
            configuration.TenantId,
            configuration.ClientId,
            configuration.ClientSecret ?? string.Empty,
            cancellationToken);

        if (!tokenResult.IsSuccess || string.IsNullOrWhiteSpace(tokenResult.AccessToken))
        {
            _logger.LogWarning(
                "Falha ao obter token Microsoft Graph para agenda do usuario {PortalUserId}: {Message}",
                user.Id,
                tokenResult.Message);
            return Array.Empty<MicrosoftGraphCalendarEvent>();
        }

        var events = await FetchCalendarEventsAsync(
            tokenResult.AccessToken,
            userIdentifier,
            limit,
            cancellationToken);

        return await _userPhotoService.EnrichEventsWithParticipantPhotosAsync(
            tokenResult.AccessToken,
            events,
            cancellationToken);
    }

    private async Task<IReadOnlyList<MicrosoftGraphCalendarEvent>> FetchCalendarEventsAsync(
        string accessToken,
        string userIdentifier,
        int limit,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var endUtc = nowUtc.AddDays(DefaultLookaheadDays);
        var startLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, SaoPauloTimeZone);
        var endLocal = TimeZoneInfo.ConvertTimeFromUtc(endUtc, SaoPauloTimeZone);

        var requestUri =
            $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(userIdentifier)}/calendarView" +
            $"?startDateTime={Uri.EscapeDataString(startLocal.ToString("yyyy-MM-ddTHH:mm:ss"))}" +
            $"&endDateTime={Uri.EscapeDataString(endLocal.ToString("yyyy-MM-ddTHH:mm:ss"))}" +
            $"&$top={limit}" +
            "&$orderby=start/dateTime" +
            "&$select=id,subject,bodyPreview,location,start,end,isAllDay,onlineMeeting,webLink,organizer,attendees";

        var client = _httpClientFactory.CreateClient(nameof(MicrosoftGraphCalendarService));
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("Prefer", $"outlook.timezone=\"{GetGraphTimeZoneId()}\"");

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao consultar calendarView no Microsoft Graph para {UserIdentifier}.", userIdentifier);
            return Array.Empty<MicrosoftGraphCalendarEvent>();
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Microsoft Graph calendarView retornou HTTP {StatusCode} para {UserIdentifier}: {Body}",
                (int)response.StatusCode,
                userIdentifier,
                body);
            return Array.Empty<MicrosoftGraphCalendarEvent>();
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (!document.RootElement.TryGetProperty("value", out var valueElement) ||
                valueElement.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<MicrosoftGraphCalendarEvent>();
            }

            var events = new List<MicrosoftGraphCalendarEvent>();
            foreach (var item in valueElement.EnumerateArray())
            {
                var mapped = MapGraphEvent(item, nowUtc);
                if (mapped is not null)
                {
                    events.Add(mapped);
                }
            }

            return events
                .OrderBy(item => item.StartAtUtc)
                .Take(limit)
                .ToList();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Resposta invalida do Microsoft Graph calendarView para {UserIdentifier}.", userIdentifier);
            return Array.Empty<MicrosoftGraphCalendarEvent>();
        }
    }

    private static MicrosoftGraphCalendarEvent? MapGraphEvent(JsonElement item, DateTime nowUtc)
    {
        var title = item.TryGetProperty("subject", out var subjectElement)
            ? subjectElement.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        if (!TryReadDateTimeOffset(item, "start", out var startAtUtc))
        {
            return null;
        }

        if (!TryReadDateTimeOffset(item, "end", out var endAtUtc))
        {
            endAtUtc = startAtUtc.AddHours(1);
        }

        if (endAtUtc <= nowUtc)
        {
            return null;
        }

        var id = item.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
        var description = item.TryGetProperty("bodyPreview", out var bodyElement) ? bodyElement.GetString() : null;
        var location = item.TryGetProperty("location", out var locationElement) &&
                       locationElement.TryGetProperty("displayName", out var locationNameElement)
            ? locationNameElement.GetString()
            : null;
        var joinUrl = ReadJoinUrl(item);
        var participants = ReadParticipants(item);
        var isAllDay = item.TryGetProperty("isAllDay", out var allDayElement) && allDayElement.GetBoolean();

        return new MicrosoftGraphCalendarEvent(
            id ?? Guid.NewGuid().ToString("N"),
            title.Trim(),
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            string.IsNullOrWhiteSpace(location) ? null : location.Trim(),
            joinUrl,
            participants,
            startAtUtc,
            endAtUtc,
            isAllDay);
    }

    private static IReadOnlyList<AgendaParticipantDto> ReadParticipants(JsonElement item)
    {
        var participants = new List<AgendaParticipantDto>();
        var seenEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (item.TryGetProperty("organizer", out var organizerElement) &&
            TryReadEmailAddress(organizerElement, out var organizerName, out var organizerEmail))
        {
            participants.Add(new AgendaParticipantDto(
                organizerName,
                organizerEmail,
                "Organizador",
                "Organizador"));

            if (!string.IsNullOrWhiteSpace(organizerEmail))
            {
                seenEmails.Add(organizerEmail);
            }
        }

        if (item.TryGetProperty("attendees", out var attendeesElement) &&
            attendeesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var attendee in attendeesElement.EnumerateArray())
            {
                if (!TryReadEmailAddress(attendee, out var name, out var email))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(email) && !seenEmails.Add(email))
                {
                    continue;
                }

                var response = attendee.TryGetProperty("status", out var statusElement) &&
                                 statusElement.TryGetProperty("response", out var responseElement)
                    ? responseElement.GetString()
                    : null;
                var type = attendee.TryGetProperty("type", out var typeElement)
                    ? typeElement.GetString()
                    : null;

                participants.Add(new AgendaParticipantDto(
                    name,
                    email,
                    MapAttendeeType(type),
                    MapResponseStatus(response)));
            }
        }

        return participants;
    }

    private static bool TryReadEmailAddress(JsonElement element, out string name, out string email)
    {
        name = string.Empty;
        email = string.Empty;

        if (!element.TryGetProperty("emailAddress", out var emailAddressElement))
        {
            return false;
        }

        name = emailAddressElement.TryGetProperty("name", out var nameElement)
            ? nameElement.GetString()?.Trim() ?? string.Empty
            : string.Empty;
        email = emailAddressElement.TryGetProperty("address", out var addressElement)
            ? addressElement.GetString()?.Trim() ?? string.Empty
            : string.Empty;

        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            name = email;
        }

        return true;
    }

    private static string MapAttendeeType(string? type)
    {
        return type?.ToLowerInvariant() switch
        {
            "optional" => "Opcional",
            "resource" => "Recurso",
            _ => "Obrigatorio"
        };
    }

    private static string MapResponseStatus(string? response)
    {
        return response?.ToLowerInvariant() switch
        {
            "accepted" => "Confirmado",
            "declined" => "Recusado",
            "tentativelyaccepted" => "Talvez",
            "none" => "Pendente",
            _ => "Pendente"
        };
    }

    private static string? ReadJoinUrl(JsonElement item)
    {
        if (item.TryGetProperty("onlineMeeting", out var onlineMeetingElement) &&
            onlineMeetingElement.TryGetProperty("joinUrl", out var joinUrlElement))
        {
            var joinUrl = joinUrlElement.GetString();
            if (!string.IsNullOrWhiteSpace(joinUrl))
            {
                return joinUrl.Trim();
            }
        }

        if (item.TryGetProperty("webLink", out var webLinkElement))
        {
            var webLink = webLinkElement.GetString();
            if (!string.IsNullOrWhiteSpace(webLink))
            {
                return webLink.Trim();
            }
        }

        return null;
    }

    private static bool TryReadDateTimeOffset(JsonElement item, string propertyName, out DateTime utcDateTime)
    {
        utcDateTime = default;
        if (!item.TryGetProperty(propertyName, out var dateElement))
        {
            return false;
        }

        var rawDateTime = dateElement.TryGetProperty("dateTime", out var dateTimeElement)
            ? dateTimeElement.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(rawDateTime))
        {
            return false;
        }

        var timeZoneId = dateElement.TryGetProperty("timeZone", out var timeZoneElement)
            ? timeZoneElement.GetString()
            : null;

        if (!DateTimeOffset.TryParse(rawDateTime, out var parsed))
        {
            return false;
        }

        if (string.Equals(timeZoneId, "UTC", StringComparison.OrdinalIgnoreCase))
        {
            utcDateTime = parsed.UtcDateTime;
            return true;
        }

        try
        {
            var sourceTimeZone = string.IsNullOrWhiteSpace(timeZoneId)
                ? SaoPauloTimeZone
                : TimeZoneInfo.FindSystemTimeZoneById(NormalizeWindowsTimeZoneId(timeZoneId!));
            var unspecified = DateTime.SpecifyKind(parsed.DateTime, DateTimeKind.Unspecified);
            utcDateTime = TimeZoneInfo.ConvertTimeToUtc(unspecified, sourceTimeZone);
            return true;
        }
        catch (Exception)
        {
            utcDateTime = parsed.UtcDateTime;
            return true;
        }
    }

    private static string? ResolveUserIdentifier(PortalUser user, string configuredIdentifier)
    {
        if (string.Equals(configuredIdentifier, "mail", StringComparison.OrdinalIgnoreCase))
        {
            return FirstNonEmpty(user.Email, user.UserPrincipalName, user.Login);
        }

        return FirstNonEmpty(user.UserPrincipalName, user.Email, user.Login);
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static string GetGraphTimeZoneId()
    {
        return OperatingSystem.IsWindows()
            ? "E. South America Standard Time"
            : "America/Sao_Paulo";
    }

    private static string NormalizeWindowsTimeZoneId(string timeZoneId)
    {
        return timeZoneId switch
        {
            "America/Sao_Paulo" => "E. South America Standard Time",
            _ => timeZoneId
        };
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
