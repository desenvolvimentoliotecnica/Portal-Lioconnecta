using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PortalRH.Api.Services;

public class MicrosoftGraphAuthClient
{
    private static readonly Uri TokenEndpointTemplate = new("https://login.microsoftonline.com/{0}/oauth2/v2.0/token");

    private readonly IHttpClientFactory _httpClientFactory;

    public MicrosoftGraphAuthClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<MicrosoftGraphAccessTokenResult> RequestAccessTokenAsync(
        string tenantId,
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return MicrosoftGraphAccessTokenResult.Failure("Tenant ID nao configurado.");
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            return MicrosoftGraphAccessTokenResult.Failure("Client ID nao configurado.");
        }

        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            return MicrosoftGraphAccessTokenResult.Failure("Client Secret nao configurado.");
        }

        var client = _httpClientFactory.CreateClient(nameof(MicrosoftGraphAuthClient));
        var tokenUri = new Uri(string.Format(TokenEndpointTemplate.ToString(), Uri.EscapeDataString(tenantId.Trim())));

        using var request = new HttpRequestMessage(HttpMethod.Post, tokenUri);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId.Trim(),
            ["client_secret"] = clientSecret,
            ["scope"] = "https://graph.microsoft.com/.default",
            ["grant_type"] = "client_credentials"
        });

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            return MicrosoftGraphAccessTokenResult.Failure($"Nao foi possivel contatar o Microsoft Entra ID: {ex.Message}");
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return MicrosoftGraphAccessTokenResult.Failure(
                "Falha ao obter token de aplicativo no Entra ID.",
                TryReadOAuthError(body) ?? $"HTTP {(int)response.StatusCode}");
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (!document.RootElement.TryGetProperty("access_token", out var tokenElement))
            {
                return MicrosoftGraphAccessTokenResult.Failure("Resposta de token recebida sem access_token.");
            }

            var accessToken = tokenElement.GetString();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return MicrosoftGraphAccessTokenResult.Failure("Resposta de token recebida com access_token vazio.");
            }

            return MicrosoftGraphAccessTokenResult.Success(accessToken);
        }
        catch (JsonException)
        {
            return MicrosoftGraphAccessTokenResult.Failure("Resposta de token do Entra ID em formato invalido.");
        }
    }

    private static string? TryReadOAuthError(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.TryGetProperty("error_description", out var description))
            {
                return description.GetString();
            }

            if (root.TryGetProperty("error", out var code))
            {
                return code.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }
}

public sealed class MicrosoftGraphAccessTokenResult
{
    public bool IsSuccess { get; init; }
    public string? AccessToken { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Detail { get; init; }

    public static MicrosoftGraphAccessTokenResult Success(string accessToken)
    {
        return new MicrosoftGraphAccessTokenResult
        {
            IsSuccess = true,
            AccessToken = accessToken
        };
    }

    public static MicrosoftGraphAccessTokenResult Failure(string message, string? detail = null)
    {
        return new MicrosoftGraphAccessTokenResult
        {
            IsSuccess = false,
            Message = message,
            Detail = detail
        };
    }
}
