using Dapper;
using PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public class TotvsRmTeamRepository : ITotvsRmTeamRepository
{
    private readonly TotvsRmQueryExecutor _queryExecutor;

    public TotvsRmTeamRepository(TotvsRmQueryExecutor queryExecutor)
    {
        _queryExecutor = queryExecutor;
    }

    public Task<IReadOnlyList<RmTeamMemberRecord>> GetTeamMembersAsync(string codSecao, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(codSecao))
        {
            return Task.FromResult<IReadOnlyList<RmTeamMemberRecord>>([]);
        }

        const string sql = """
            SELECT
                LTRIM(RTRIM(F.CHAPA)) AS Chapa,
                LTRIM(RTRIM(F.NOME)) AS Name,
                COALESCE(LTRIM(RTRIM(FN.NOME)), '—') AS Role,
                COALESCE(LTRIM(RTRIM(S.DESCRICAO)), '—') AS Department,
                CASE WHEN F.CODSITUACAO = 'A' OR F.CODSITUACAO IS NULL THEN 'Ativo' ELSE 'Inativo' END AS Status
            FROM dbo.PFUNC F WITH (NOLOCK)
            LEFT JOIN dbo.PFUNCAO FN WITH (NOLOCK)
                ON FN.CODCOLIGADA = F.CODCOLIGADA AND FN.CODIGO = F.CODFUNCAO
            LEFT JOIN dbo.PSECAO S WITH (NOLOCK)
                ON S.CODCOLIGADA = F.CODCOLIGADA AND S.CODIGO = F.CODSECAO
            WHERE F.CODCOLIGADA = @CodColigada
              AND F.CODSECAO = @CodSecao
              AND (F.CODSITUACAO IS NULL OR F.CODSITUACAO = 'A')
            ORDER BY F.NOME;
            """;

        return _queryExecutor.QueryAsync(
            "PFUNC team",
            async (runtime, connection, token) =>
            {
                var rows = await connection.QueryAsync<RmTeamMemberRecord>(sql, new
                {
                    CodColigada = runtime.CodColigada,
                    CodSecao = codSecao
                });
                return (IReadOnlyList<RmTeamMemberRecord>)rows.ToList();
            },
            cancellationToken);
    }
}
