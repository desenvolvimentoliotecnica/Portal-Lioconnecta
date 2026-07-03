using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortalLioConnecta.Api.Data;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Services;

public class PortalUserEmployeeIdResolver : IPortalUserEmployeeIdResolver
{
    private readonly PortalLioConnectaDbContext _dbContext;
    private readonly IMicrosoftGraphConfigurationService _graphConfigurationService;
    private readonly MicrosoftGraphAuthClient _graphAuthClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PortalUserEmployeeIdResolver> _logger;

    public PortalUserEmployeeIdResolver(
        PortalLioConnectaDbContext dbContext,
        IMicrosoftGraphConfigurationService graphConfigurationService,
        MicrosoftGraphAuthClient graphAuthClient,
        IHttpClientFactory httpClientFactory,
        ILogger<PortalUserEmployeeIdResolver> logger)
    {
        _dbContext = dbContext;
        _graphConfigurationService = graphConfigurationService;
        _graphAuthClient = graphAuthClient;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<PortalUserEmployeeIdResolution> ResolveAsync(
        PortalUser user,
        bool persistWhenFound,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(user.EmployeeId))
        {
            return new PortalUserEmployeeIdResolution
            {
                EmployeeId = user.EmployeeId.Trim(),
                Source = "portal_user"
            };
        }

        var graphProfile = await FetchGraphProfileAsync(user, cancellationToken);
        if (string.IsNullOrWhiteSpace(graphProfile?.EmployeeId))
        {
            return new PortalUserEmployeeIdResolution
            {
                EmployeeId = null,
                Source = "none"
            };
        }

        var employeeId = graphProfile.EmployeeId.Trim();
        if (persistWhenFound)
        {
            await PersistAsync(user, employeeId, graphProfile, cancellationToken);
        }

        return new PortalUserEmployeeIdResolution
        {
            EmployeeId = employeeId,
            Source = "microsoft_graph"
        };
    }

    private async Task PersistAsync(
        PortalUser user,
        string employeeId,
        GraphUserProfile graphProfile,
        CancellationToken cancellationToken)
    {
        var trackedUser = await _dbContext.PortalUsers
            .FirstOrDefaultAsync(item => item.Id == user.Id, cancellationToken);

        if (trackedUser is null)
        {
            return;
        }

        var changed = false;

        if (string.IsNullOrWhiteSpace(trackedUser.EmployeeId))
        {
            trackedUser.EmployeeId = employeeId;
            user.EmployeeId = employeeId;
            changed = true;
        }

        if (string.IsNullOrWhiteSpace(trackedUser.Title) && !string.IsNullOrWhiteSpace(graphProfile.JobTitle))
        {
            trackedUser.Title = graphProfile.JobTitle.Trim();
            user.Title = trackedUser.Title;
            changed = true;
        }

        if (string.IsNullOrWhiteSpace(trackedUser.Department) && !string.IsNullOrWhiteSpace(graphProfile.Department))
        {
            trackedUser.Department = graphProfile.Department.Trim();
            user.Department = trackedUser.Department;
            changed = true;
        }

        if (!changed)
        {
            return;
        }

        trackedUser.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Matricula {EmployeeId} sincronizada via Microsoft Graph para o usuario {PortalUserId}.",
            employeeId,
            user.Id);
    }

    private async Task<GraphUserProfile?> FetchGraphProfileAsync(
        PortalUser user,
        CancellationToken cancellationToken)
    {
        var configuration = await _graphConfigurationService.GetRuntimeConfigurationAsync(cancellationToken);
        if (!configuration.IsEnabled)
        {
            return null;
        }

        var userIdentifier = ResolveUserIdentifier(user, configuration.UserIdentifier);
        if (string.IsNullOrWhiteSpace(userIdentifier))
        {
            return null;
        }

        var tokenResult = await _graphAuthClient.RequestAccessTokenAsync(
            configuration.TenantId,
            configuration.ClientId,
            configuration.ClientSecret ?? string.Empty,
            cancellationToken);

        if (!tokenResult.IsSuccess || string.IsNullOrWhiteSpace(tokenResult.AccessToken))
        {
            _logger.LogWarning(
                "Nao foi possivel obter token Microsoft Graph para resolver matricula do usuario {PortalUserId}: {Message}",
                user.Id,
                tokenResult.Message);
            return null;
        }

        var profile = await FetchGraphProfileByIdentifierAsync(tokenResult.AccessToken, userIdentifier, cancellationToken);
        if (profile is not null)
        {
            return profile;
        }

        var mail = FirstNonEmpty(user.Email, user.UserPrincipalName);
        if (string.IsNullOrWhiteSpace(mail) ||
            string.Equals(mail, userIdentifier, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return await FetchGraphProfileByMailFilterAsync(tokenResult.AccessToken, mail, cancellationToken);
    }

    private async Task<GraphUserProfile?> FetchGraphProfileByIdentifierAsync(
        string accessToken,
        string userIdentifier,
        CancellationToken cancellationToken)
    {
        var requestUri =
            $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(userIdentifier)}?$select=employeeId,displayName,jobTitle,department,mail,userPrincipalName";

        return await SendGraphProfileRequestAsync(accessToken, requestUri, cancellationToken);
    }

    private async Task<GraphUserProfile?> FetchGraphProfileByMailFilterAsync(
        string accessToken,
        string mail,
        CancellationToken cancellationToken)
    {
        var requestUri =
            $"https://graph.microsoft.com/v1.0/users?$filter=mail eq '{EscapeODataString(mail)}'&$select=employeeId,displayName,jobTitle,department,mail,userPrincipalName&$top=1";

        var client = _httpClientFactory.CreateClient(nameof(PortalUserEmployeeIdResolver));
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Falha ao consultar Microsoft Graph por mail {Mail}.", mail);
            return null;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Microsoft Graph users filter retornou HTTP {StatusCode} para {Mail}: {Body}",
                (int)response.StatusCode,
                mail,
                body);
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (!document.RootElement.TryGetProperty("value", out var valueElement) ||
                valueElement.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (var item in valueElement.EnumerateArray())
            {
                return MapGraphProfile(item);
            }
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "Resposta invalida do Microsoft Graph users filter para {Mail}.", mail);
        }

        return null;
    }

    private async Task<GraphUserProfile?> SendGraphProfileRequestAsync(
        string accessToken,
        string requestUri,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(nameof(PortalUserEmployeeIdResolver));
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Falha ao consultar Microsoft Graph em {RequestUri}.", requestUri);
            return null;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Microsoft Graph users retornou HTTP {StatusCode} para {RequestUri}: {Body}",
                (int)response.StatusCode,
                requestUri,
                body);
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            return MapGraphProfile(document.RootElement);
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "Resposta invalida do Microsoft Graph em {RequestUri}.", requestUri);
            return null;
        }
    }

    private static GraphUserProfile? MapGraphProfile(JsonElement item)
    {
        var employeeId = ReadStringProperty(item, "employeeId");
        if (string.IsNullOrWhiteSpace(employeeId))
        {
            return null;
        }

        return new GraphUserProfile(
            employeeId.Trim(),
            ReadStringProperty(item, "displayName"),
            ReadStringProperty(item, "jobTitle"),
            ReadStringProperty(item, "department"));
    }

    private static string? ReadStringProperty(JsonElement item, string propertyName)
    {
        return item.TryGetProperty(propertyName, out var property) &&
               property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
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

    private static string EscapeODataString(string value) =>
        value.Replace("'", "''", StringComparison.Ordinal);

    private sealed record GraphUserProfile(
        string EmployeeId,
        string? DisplayName,
        string? JobTitle,
        string? Department);
}
