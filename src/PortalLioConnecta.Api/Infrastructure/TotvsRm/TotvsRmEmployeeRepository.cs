using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public class TotvsRmEmployeeRepository : ITotvsRmEmployeeRepository
{
    private readonly ITotvsRmConfigurationService _configurationService;
    private readonly ILogger<TotvsRmEmployeeRepository> _logger;

    public TotvsRmEmployeeRepository(
        ITotvsRmConfigurationService configurationService,
        ILogger<TotvsRmEmployeeRepository> logger)
    {
        _configurationService = configurationService;
        _logger = logger;
    }

    public async Task<string?> LookupChapaByFullNameAsync(string fullName, CancellationToken cancellationToken)
    {
        var normalizedName = TotvsRmEmployeeNameNormalizer.NormalizeForComparison(fullName);
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return null;
        }

        const string sql = """
            SELECT TOP 2 LTRIM(RTRIM(F.CHAPA)) AS Chapa
            FROM dbo.PFUNC F WITH (NOLOCK)
            WHERE F.CODCOLIGADA = @CodColigada
              AND UPPER(LTRIM(RTRIM(F.NOME))) = @NomeNormalizado
              AND (F.CODSITUACAO IS NULL OR F.CODSITUACAO = 'A')
            ORDER BY F.CHAPA;
            """;

        try
        {
            var runtime = await _configurationService.GetRuntimeConfigurationAsync(cancellationToken);
            if (!runtime.IsEnabled)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(runtime.Password))
            {
                return null;
            }

            await using var connection = TotvsRmConnectionFactory.CreateConnection(runtime);
            await connection.OpenAsync(cancellationToken);

            var rows = (await connection.QueryAsync<string>(sql, new
            {
                CodColigada = TotvsRmConstants.CodColigada,
                NomeNormalizado = normalizedName
            })).ToList();

            if (rows.Count == 0)
            {
                _logger.LogInformation(
                    "Nenhuma CHAPA encontrada em PFUNC para o nome {FullName}.",
                    fullName);
                return null;
            }

            if (rows.Count > 1)
            {
                _logger.LogWarning(
                    "Nome {FullName} retornou {MatchCount} CHAPAs em PFUNC ({Chapas}). Resolucao ignorada por ambiguidade.",
                    fullName,
                    rows.Count,
                    string.Join(", ", rows));
                return null;
            }

            var chapa = rows[0]?.Trim();
            return string.IsNullOrWhiteSpace(chapa) ? null : chapa;
        }
        catch (TotvsRmIntegrationException exception)
        {
            _logger.LogWarning(
                exception,
                "Falha ao consultar PFUNC para resolver matricula pelo nome {FullName}.",
                fullName);
            return null;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Erro inesperado ao consultar PFUNC para o nome {FullName}.",
                fullName);
            return null;
        }
    }
}
