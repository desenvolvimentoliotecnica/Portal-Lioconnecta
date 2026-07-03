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
    private readonly ILdapConfigurationService _ldapConfigurationService;
    private readonly ILdapDirectoryAuthenticator _ldapDirectoryAuthenticator;
    private readonly IMicrosoftGraphConfigurationService _graphConfigurationService;
    private readonly MicrosoftGraphAuthClient _graphAuthClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PortalUserEmployeeIdResolver> _logger;

    public PortalUserEmployeeIdResolver(
        PortalLioConnectaDbContext dbContext,
        ILdapConfigurationService ldapConfigurationService,
        ILdapDirectoryAuthenticator ldapDirectoryAuthenticator,
        IMicrosoftGraphConfigurationService graphConfigurationService,
        MicrosoftGraphAuthClient graphAuthClient,
        IHttpClientFactory httpClientFactory,
        ILogger<PortalUserEmployeeIdResolver> logger)
    {
        _dbContext = dbContext;
        _ldapConfigurationService = ldapConfigurationService;
        _ldapDirectoryAuthenticator = ldapDirectoryAuthenticator;
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

        var ldapProfile = await FetchLdapProfileAsync(user, cancellationToken);
        if (!string.IsNullOrWhiteSpace(ldapProfile?.EmployeeId))
        {
            var employeeId = ldapProfile.EmployeeId.Trim();
            if (persistWhenFound)
            {
                await PersistAsync(user, employeeId, ldapProfile.Title, ldapProfile.Department, "ldap", cancellationToken);
            }

            return new PortalUserEmployeeIdResolution
            {
                EmployeeId = employeeId,
                Source = "ldap"
            };
        }

        var graphProfile = await FetchGraphProfileAsync(user, cancellationToken);
        if (string.IsNullOrWhiteSpace(graphProfile?.EmployeeId))
        {
            _logger.LogWarning(
                "Matricula nao encontrada para o usuario {PortalUserId} ({Login}). Fontes tentadas: portal_user, ldap, microsoft_graph.",
                user.Id,
                FirstNonEmpty(user.UserPrincipalName, user.Email, user.Login));

            return new PortalUserEmployeeIdResolution
            {
                EmployeeId = null,
                Source = "none"
            };
        }

        var graphEmployeeId = graphProfile.EmployeeId.Trim();
        if (persistWhenFound)
        {
            await PersistAsync(
                user,
                graphEmployeeId,
                graphProfile.JobTitle,
                graphProfile.Department,
                "microsoft_graph",
                cancellationToken);
        }

        return new PortalUserEmployeeIdResolution
        {
            EmployeeId = graphEmployeeId,
            Source = "microsoft_graph"
        };
    }

    private async Task<LdapProfileSnapshot?> FetchLdapProfileAsync(
        PortalUser user,
        CancellationToken cancellationToken)
    {
        var configuration = await _ldapConfigurationService.GetRuntimeConfigurationAsync(cancellationToken);
        if (!configuration.IsEnabled)
        {
            return null;
        }

        var candidates = BuildLookupCandidates(user);
        var ldapUser = await _ldapDirectoryAuthenticator.LookupUserProfileAsync(
            configuration,
            candidates,
            cancellationToken);

        if (ldapUser is null || string.IsNullOrWhiteSpace(ldapUser.EmployeeId))
        {
            return null;
        }

        return new LdapProfileSnapshot(
            ldapUser.EmployeeId.Trim(),
            ldapUser.Title,
            ldapUser.Department);
    }

    private async Task PersistAsync(
        PortalUser user,
        string employeeId,
        string? title,
        string? department,
        string source,
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

        if (string.IsNullOrWhiteSpace(trackedUser.Title) && !string.IsNullOrWhiteSpace(title))
        {
            trackedUser.Title = title.Trim();
            user.Title = trackedUser.Title;
            changed = true;
        }

        if (string.IsNullOrWhiteSpace(trackedUser.Department) && !string.IsNullOrWhiteSpace(department))
        {
            trackedUser.Department = department.Trim();
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
            "Matricula {EmployeeId} sincronizada via {Source} para o usuario {PortalUserId}.",
            employeeId,
            source,
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

        var accessToken = tokenResult.AccessToken;
        var userIdentifier = ResolveUserIdentifier(user, configuration.UserIdentifier);
        if (!string.IsNullOrWhiteSpace(userIdentifier))
        {
            var profile = await FetchGraphProfileByIdentifierAsync(accessToken, userIdentifier, cancellationToken);
            if (profile is not null)
            {
                return profile;
            }

            profile = await FetchGraphProfileByFilterAsync(
                accessToken,
                $"userPrincipalName eq '{EscapeODataString(userIdentifier)}'",
                cancellationToken);
            if (profile is not null)
            {
                return profile;
            }
        }

        var mail = FirstNonEmpty(user.Email, user.UserPrincipalName);
        if (!string.IsNullOrWhiteSpace(mail))
        {
            var profile = await FetchGraphProfileByFilterAsync(
                accessToken,
                $"mail eq '{EscapeODataString(mail)}'",
                cancellationToken);
            if (profile is not null)
            {
                return profile;
            }
        }

        var login = FirstNonEmpty(user.SamAccountName, user.Login);
        if (!string.IsNullOrWhiteSpace(login) && !login.Contains('@', StringComparison.Ordinal))
        {
            return await FetchGraphProfileByFilterAsync(
                accessToken,
                $"startswith(userPrincipalName,'{EscapeODataString(login)}@')",
                cancellationToken);
        }

        return null;
    }

    private async Task<GraphUserProfile?> FetchGraphProfileByIdentifierAsync(
        string accessToken,
        string userIdentifier,
        CancellationToken cancellationToken)
    {
        var requestUri =
            $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(userIdentifier)}?$select={GraphUserSelectFields}";

        return await SendGraphProfileRequestAsync(accessToken, requestUri, cancellationToken);
    }

    private async Task<GraphUserProfile?> FetchGraphProfileByFilterAsync(
        string accessToken,
        string filterExpression,
        CancellationToken cancellationToken)
    {
        var requestUri =
            $"https://graph.microsoft.com/v1.0/users?$filter={Uri.EscapeDataString(filterExpression)}&$select={GraphUserSelectFields}&$top=1";

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
            _logger.LogWarning(exception, "Falha ao consultar Microsoft Graph com filtro {Filter}.", filterExpression);
            return null;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Microsoft Graph users filter retornou HTTP {StatusCode} para {Filter}: {Body}",
                (int)response.StatusCode,
                filterExpression,
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
            _logger.LogWarning(exception, "Resposta invalida do Microsoft Graph para filtro {Filter}.", filterExpression);
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

    public static string? TryReadEmployeeIdFromGraph(JsonElement item) =>
        MapGraphProfile(item)?.EmployeeId;

    internal static GraphUserProfile? MapGraphProfile(JsonElement item)
    {
        var employeeId = FirstNonEmpty(
            ReadStringProperty(item, "employeeId"),
            ReadStringProperty(item, "employeeNumber"));

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

    private static IEnumerable<string> BuildLookupCandidates(PortalUser user)
    {
        yield return user.Email ?? string.Empty;
        yield return user.UserPrincipalName ?? string.Empty;
        yield return user.Login ?? string.Empty;
        yield return user.SamAccountName ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(user.Login) && user.Login.Contains('\\', StringComparison.Ordinal))
        {
            yield return user.Login.Split('\\', 2)[1];
        }
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

    private const string GraphUserSelectFields =
        "employeeId,employeeNumber,displayName,jobTitle,department,mail,userPrincipalName";

    internal sealed record GraphUserProfile(
        string EmployeeId,
        string? DisplayName,
        string? JobTitle,
        string? Department);

    private sealed record LdapProfileSnapshot(
        string EmployeeId,
        string? Title,
        string? Department);
}
