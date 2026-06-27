using System.Collections.Concurrent;
using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PortalRH.Api.Contracts.Agenda;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

public class MicrosoftGraphUserPhotoService : IMicrosoftGraphUserPhotoService
{
    private static readonly TimeSpan PhotoCacheDuration = TimeSpan.FromHours(6);
    private const int MaxConcurrentPhotoRequests = 6;

    private readonly IMicrosoftGraphConfigurationService _configurationService;
    private readonly MicrosoftGraphAuthClient _authClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MicrosoftGraphUserPhotoService> _logger;

    public MicrosoftGraphUserPhotoService(
        IMicrosoftGraphConfigurationService configurationService,
        MicrosoftGraphAuthClient authClient,
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        ILogger<MicrosoftGraphUserPhotoService> logger)
    {
        _configurationService = configurationService;
        _authClient = authClient;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<string?> GetPhotoDataUrlForPortalUserAsync(
        PortalUser user,
        CancellationToken cancellationToken)
    {
        var configuration = await _configurationService.GetRuntimeConfigurationAsync(cancellationToken);
        if (!configuration.IsEnabled)
        {
            return null;
        }

        var userIdentifier = ResolveUserIdentifier(user, configuration.UserIdentifier);
        if (string.IsNullOrWhiteSpace(userIdentifier))
        {
            return null;
        }

        var tokenResult = await _authClient.RequestAccessTokenAsync(
            configuration.TenantId,
            configuration.ClientId,
            configuration.ClientSecret ?? string.Empty,
            cancellationToken);

        if (!tokenResult.IsSuccess || string.IsNullOrWhiteSpace(tokenResult.AccessToken))
        {
            return null;
        }

        return await ResolvePhotoDataUrlAsync(tokenResult.AccessToken, userIdentifier, cancellationToken);
    }

    public async Task<IReadOnlyList<MicrosoftGraphCalendarEvent>> EnrichEventsWithParticipantPhotosAsync(
        string accessToken,
        IReadOnlyList<MicrosoftGraphCalendarEvent> events,
        CancellationToken cancellationToken)
    {
        if (events.Count == 0 || string.IsNullOrWhiteSpace(accessToken))
        {
            return events;
        }

        var emails = events
            .SelectMany(item => item.Participants)
            .Select(participant => participant.Email?.Trim() ?? string.Empty)
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (emails.Count == 0)
        {
            return events;
        }

        var photoUrls = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        using var gate = new SemaphoreSlim(MaxConcurrentPhotoRequests);

        await Task.WhenAll(emails.Select(async email =>
        {
            await gate.WaitAsync(cancellationToken);
            try
            {
                var photoUrl = await ResolvePhotoDataUrlAsync(accessToken, email, cancellationToken);
                if (!string.IsNullOrWhiteSpace(photoUrl))
                {
                    photoUrls[email] = photoUrl;
                }
            }
            finally
            {
                gate.Release();
            }
        }));

        if (photoUrls.IsEmpty)
        {
            return events;
        }

        return events
            .Select(item => item with
            {
                Participants = item.Participants
                    .Select(participant => participant with
                    {
                        PhotoUrl = !string.IsNullOrWhiteSpace(participant.Email) &&
                                   photoUrls.TryGetValue(participant.Email, out var photoUrl)
                            ? photoUrl
                            : participant.PhotoUrl
                    })
                    .ToList()
            })
            .ToList();
    }

    private async Task<string?> ResolvePhotoDataUrlAsync(
        string accessToken,
        string email,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"msgraph-user-photo:{email.ToLowerInvariant()}";
        if (_memoryCache.TryGetValue(cacheKey, out string? cached))
        {
            return string.IsNullOrWhiteSpace(cached) ? null : cached;
        }

        var photoUrl = await FetchPhotoDataUrlAsync(accessToken, email, cancellationToken);
        _memoryCache.Set(cacheKey, photoUrl ?? string.Empty, PhotoCacheDuration);
        return photoUrl;
    }

    private async Task<string?> FetchPhotoDataUrlAsync(
        string accessToken,
        string email,
        CancellationToken cancellationToken)
    {
        var requestUri =
            $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(email)}/photos/48x48/$value";

        var client = _httpClientFactory.CreateClient(nameof(MicrosoftGraphUserPhotoService));
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Falha ao consultar foto do usuario {Email} no Microsoft Graph.", email);
            return null;
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogDebug(
                "Microsoft Graph retornou HTTP {StatusCode} ao consultar foto do usuario {Email}.",
                (int)response.StatusCode,
                email);
            return null;
        }

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        if (bytes.Length == 0)
        {
            return null;
        }

        return $"data:{contentType};base64,{Convert.ToBase64String(bytes)}";
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
}
