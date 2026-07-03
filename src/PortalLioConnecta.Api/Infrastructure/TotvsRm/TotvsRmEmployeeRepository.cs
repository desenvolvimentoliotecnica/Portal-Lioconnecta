using Dapper;
using Microsoft.Extensions.Logging;
using PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public class TotvsRmEmployeeRepository : ITotvsRmEmployeeRepository
{
    private readonly TotvsRmQueryExecutor _queryExecutor;
    private readonly ILogger<TotvsRmEmployeeRepository> _logger;

    public TotvsRmEmployeeRepository(
        TotvsRmQueryExecutor queryExecutor,
        ILogger<TotvsRmEmployeeRepository> logger)
    {
        _queryExecutor = queryExecutor;
        _logger = logger;
    }

    public Task<string?> LookupChapaByFullNameAsync(string fullName, CancellationToken cancellationToken)
    {
        var normalizedName = TotvsRmEmployeeNameNormalizer.NormalizeForComparison(fullName);
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return Task.FromResult<string?>(null);
        }

        const string sql = """
            SELECT TOP 2 LTRIM(RTRIM(F.CHAPA)) AS Chapa
            FROM dbo.PFUNC F WITH (NOLOCK)
            WHERE F.CODCOLIGADA = @CodColigada
              AND UPPER(LTRIM(RTRIM(F.NOME))) = @NomeNormalizado
              AND (F.CODSITUACAO IS NULL OR F.CODSITUACAO = 'A')
            ORDER BY F.CHAPA;
            """;

        return _queryExecutor.TryQueryAsync(
            "PFUNC lookup by name",
            async (runtime, connection, token) =>
            {
                var rows = (await connection.QueryAsync<string>(sql, new
                {
                    CodColigada = runtime.CodColigada,
                    NomeNormalizado = normalizedName
                })).ToList();

                if (rows.Count == 0)
                {
                    _logger.LogInformation("Nenhuma CHAPA encontrada em PFUNC para o nome {FullName}.", fullName);
                    return null;
                }

                if (rows.Count > 1)
                {
                    _logger.LogWarning(
                        "Nome {FullName} retornou {MatchCount} CHAPAs em PFUNC. Resolucao ignorada.",
                        fullName,
                        rows.Count);
                    return null;
                }

                var chapa = rows[0]?.Trim();
                return string.IsNullOrWhiteSpace(chapa) ? null : chapa;
            },
            cancellationToken,
            throwWhenDisabled: false);
    }

    public Task<TotvsRmHrContext?> GetHrContextByChapaAsync(string chapa, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1
                LTRIM(RTRIM(F.CHAPA)) AS Chapa,
                F.CODPESSOA AS CodPessoa,
                LTRIM(RTRIM(F.CODSECAO)) AS CodSecao
            FROM dbo.PFUNC F WITH (NOLOCK)
            WHERE F.CODCOLIGADA = @CodColigada
              AND F.CHAPA = @Chapa
              AND (F.CODSITUACAO IS NULL OR F.CODSITUACAO = 'A');
            """;

        return _queryExecutor.TryQueryAsync(
            "PFUNC hr context",
            async (runtime, connection, token) =>
            {
                var row = await connection.QueryFirstOrDefaultAsync<TotvsRmHrContext>(sql, new
                {
                    CodColigada = runtime.CodColigada,
                    Chapa = chapa
                });
                return row;
            },
            cancellationToken,
            throwWhenDisabled: false);
    }

    public Task<RmEmployeeProfileRecord?> GetProfileByChapaAsync(string chapa, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1
                LTRIM(RTRIM(F.CHAPA)) AS Chapa,
                F.CODPESSOA AS CodPessoa,
                LTRIM(RTRIM(F.NOME)) AS Nome,
                LTRIM(RTRIM(F.CODSECAO)) AS CodSecao,
                LTRIM(RTRIM(S.DESCRICAO)) AS SecaoDescricao,
                LTRIM(RTRIM(F.CODFUNCAO)) AS CodFuncao,
                LTRIM(RTRIM(FN.NOME)) AS FuncaoDescricao,
                F.DATAADMISSAO AS DataAdmissao,
                LTRIM(RTRIM(P.CPF)) AS Cpf,
                LTRIM(RTRIM(P.CARTIDENTIDADE)) AS Rg,
                LTRIM(RTRIM(P.TELEFONE1)) AS Telefone,
                LTRIM(RTRIM(P.EMAIL)) AS EmailPessoal,
                LTRIM(RTRIM(P.CIDADE)) AS Cidade,
                LTRIM(RTRIM(P.ESTADO)) AS Estado,
                LTRIM(RTRIM(P.RUA)) AS Endereco,
                LTRIM(RTRIM(G.NOME)) AS GestorNome,
                LTRIM(RTRIM(F.CODBANCOPAGTO)) AS Banco,
                LTRIM(RTRIM(F.CODAGENCIAPAGTO)) AS Agencia,
                LTRIM(RTRIM(F.CONTAPAGAMENTO)) AS Conta
            FROM dbo.PFUNC F WITH (NOLOCK)
            LEFT JOIN dbo.PPESSOA P WITH (NOLOCK)
                ON P.CODIGO = F.CODPESSOA
            LEFT JOIN dbo.PSECAO S WITH (NOLOCK)
                ON S.CODCOLIGADA = F.CODCOLIGADA AND S.CODIGO = F.CODSECAO
            LEFT JOIN dbo.PFUNCAO FN WITH (NOLOCK)
                ON FN.CODCOLIGADA = F.CODCOLIGADA AND FN.CODIGO = F.CODFUNCAO
            LEFT JOIN dbo.PFUNC G WITH (NOLOCK)
                ON G.CODCOLIGADA = S.CODCOLIGADA AND G.CHAPA = S.CHAPACHEFE
            WHERE F.CODCOLIGADA = @CodColigada
              AND F.CHAPA = @Chapa
              AND (F.CODSITUACAO IS NULL OR F.CODSITUACAO = 'A');
            """;

        return _queryExecutor.TryQueryAsync(
            "PFUNC profile",
            async (runtime, connection, token) =>
            {
                return await connection.QueryFirstOrDefaultAsync<RmEmployeeProfileRecord>(sql, new
                {
                    CodColigada = runtime.CodColigada,
                    Chapa = chapa
                });
            },
            cancellationToken,
            throwWhenDisabled: false);
    }
}
