using System.Net.Http.Headers;
using System.Text.Json;
using PortalRH.Api.Contracts.Admin.MicrosoftGraph;

namespace PortalRH.Api.Services;

public class MicrosoftGraphConnectionTester
{
    private static readonly Uri TokenEndpointTemplate = new("https://login.microsoftonline.com/{0}/oauth2/v2.0/token");
    private static readonly Uri GraphUsersEndpoint = new("https://graph.microsoft.com/v1.0/users?$select=id,displayName&$top=1");

    private readonly IHttpClientFactory _httpClientFactory;

    public MicrosoftGraphConnectionTester(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<MicrosoftGraphConnectionTestResponse> TestAsync(
        string tenantId,
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return Failure("Informe o Tenant ID para testar a conexao.");
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            return Failure("Informe o Client ID para testar a conexao.");
        }

        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            return Failure("Informe o Client Secret ou salve a configuracao antes de testar.");
        }

        var client = _httpClientFactory.CreateClient(nameof(MicrosoftGraphConnectionTester));

        var tokenResult = await RequestAccessTokenAsync(client, tenantId, clientId, clientSecret, cancellationToken);
        if (!tokenResult.Success)
        {
            return tokenResult.Error!;
        }

        return await ValidateGraphAccessAsync(client, tokenResult.AccessToken!, cancellationToken);
    }

    private static async Task<TokenRequestResult> RequestAccessTokenAsync(
        HttpClient client,
        string tenantId,
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken)
    {
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
            return TokenRequestResult.Failed($"Nao foi possivel contatar o Microsoft Entra ID: {ex.Message}");
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var detail = TryReadOAuthError(body) ?? $"HTTP {(int)response.StatusCode}";
            return TokenRequestResult.Failed("Falha ao obter token de aplicativo no Entra ID.", detail);
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (!document.RootElement.TryGetProperty("access_token", out var tokenElement))
            {
                return TokenRequestResult.Failed("Resposta de token recebida sem access_token.");
            }

            var accessToken = tokenElement.GetString();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return TokenRequestResult.Failed("Resposta de token recebida com access_token vazio.");
            }

            return TokenRequestResult.Ok(accessToken);
        }
        catch (JsonException)
        {
            return TokenRequestResult.Failed("Resposta de token do Entra ID em formato invalido.");
        }
    }

    private static async Task<MicrosoftGraphConnectionTestResponse> ValidateGraphAccessAsync(
        HttpClient client,
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, GraphUsersEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            return Failure("Token obtido, mas a API Microsoft Graph nao respondeu.", ex.Message);
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return Failure(
                "Token obtido, mas o Graph recusou a consulta.",
                "Revise o admin consent das permissoes Calendars.Read e User.Read.All.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return Failure(
                "Token obtido, mas a validacao no Graph falhou.",
                TryReadGraphError(body) ?? $"HTTP {(int)response.StatusCode}");
        }

        return new MicrosoftGraphConnectionTestResponse(
            true,
            "Conexao validada com sucesso no Microsoft Graph.",
            "Token de aplicativo obtido e permissao de leitura confirmada.");
    }

    private static MicrosoftGraphConnectionTestResponse Failure(string message, string? detail = null)
    {
        return new MicrosoftGraphConnectionTestResponse(false, message, detail);
    }

    private static string? TryReadOAuthError(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            var error = root.TryGetProperty("error_description", out var description)
                ? description.GetString()
                : root.TryGetProperty("error", out var code)
                    ? code.GetString()
                    : null;

            return string.IsNullOrWhiteSpace(error) ? null : error;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? TryReadGraphError(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("error", out var errorElement) &&
                errorElement.TryGetProperty("message", out var messageElement))
            {
                return messageElement.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private sealed class TokenRequestResult
    {
        public bool Success { get; init; }
        public string? AccessToken { get; init; }
        public MicrosoftGraphConnectionTestResponse? Error { get; init; }

        public static TokenRequestResult Ok(string accessToken)
        {
            return new TokenRequestResult
            {
                Success = true,
                AccessToken = accessToken
            };
        }

        public static TokenRequestResult Failed(string message, string? detail = null)
        {
            return new TokenRequestResult
            {
                Success = false,
                Error = new MicrosoftGraphConnectionTestResponse(false, message, detail)
            };
        }
    }
}
