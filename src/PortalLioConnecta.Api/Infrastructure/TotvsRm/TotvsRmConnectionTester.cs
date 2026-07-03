using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using PortalLioConnecta.Api.Contracts.Admin.TotvsRm;
using PortalLioConnecta.Api.Services;

namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public class TotvsRmConnectionTester
{
    private readonly ILogger<TotvsRmConnectionTester> _logger;

    public TotvsRmConnectionTester(ILogger<TotvsRmConnectionTester> logger)
    {
        _logger = logger;
    }

    public async Task<TotvsRmConnectionTestResponse> TestAsync(
        TotvsRmRuntimeConfiguration configuration,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(configuration.Server))
        {
            return new TotvsRmConnectionTestResponse(false, "Servidor SQL nao informado.", null);
        }

        if (string.IsNullOrWhiteSpace(configuration.Database))
        {
            return new TotvsRmConnectionTestResponse(false, "Database nao informado.", null);
        }

        if (string.IsNullOrWhiteSpace(configuration.UserName))
        {
            return new TotvsRmConnectionTestResponse(false, "Usuario SQL nao informado.", null);
        }

        if (string.IsNullOrWhiteSpace(configuration.Password))
        {
            return new TotvsRmConnectionTestResponse(false, "Senha SQL nao informada.", null);
        }

        try
        {
            await using var connection = TotvsRmConnectionFactory.CreateConnection(configuration);
            await connection.OpenAsync(cancellationToken);

            await using var pingCommand = connection.CreateCommand();
            pingCommand.CommandText = "SELECT 1";
            pingCommand.CommandTimeout = 20;
            var ping = await pingCommand.ExecuteScalarAsync(cancellationToken);
            if (ping is null or not 1)
            {
                return new TotvsRmConnectionTestResponse(false, "Conexao estabelecida, mas a validacao basica falhou.", null);
            }

            var results = new List<string>();
            var failures = 0;

            foreach (var tableName in TotvsRmConstants.RequiredReadTables)
            {
                try
                {
                    var count = await CountTableAsync(connection, tableName, cancellationToken);
                    results.Add($"{tableName}: OK ({count} registros)");
                }
                catch (Exception exception)
                {
                    failures++;
                    results.Add($"{tableName}: SEM PERMISSAO ({exception.Message})");
                    _logger.LogWarning(
                        exception,
                        "Usuario {UserName} sem SELECT em {TableName}.",
                        configuration.UserName,
                        tableName);
                }
            }

            var detail = string.Join("; ", results) +
                         $". Usuario: {configuration.UserName}; CodColigada: {configuration.CodColigada}.";

            if (failures > 0)
            {
                return new TotvsRmConnectionTestResponse(
                    false,
                    $"Conexao OK, mas {failures} tabela(s) sem permissao SELECT para o usuario informado.",
                    detail);
            }

            return new TotvsRmConnectionTestResponse(
                true,
                "Conexao com TOTVS RM realizada com sucesso. Todas as tabelas necessarias estao acessiveis.",
                detail);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Falha ao testar conexao TOTVS RM.");
            return new TotvsRmConnectionTestResponse(
                false,
                "Nao foi possivel conectar ao banco TOTVS RM.",
                exception.Message);
        }
    }

    private static async Task<long> CountTableAsync(SqlConnection connection, string tableName, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT_BIG(1) FROM dbo.[{tableName}] WITH (NOLOCK)";
        command.CommandTimeout = 20;
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result switch
        {
            long longValue => longValue,
            int intValue => intValue,
            _ => 0
        };
    }
}
