using Dapper;
using PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public class TotvsRmBenefitsRepository : ITotvsRmBenefitsRepository
{
    private readonly TotvsRmQueryExecutor _queryExecutor;

    public TotvsRmBenefitsRepository(TotvsRmQueryExecutor queryExecutor)
    {
        _queryExecutor = queryExecutor;
    }

    public Task<IReadOnlyList<RmBenefitRecord>> GetBenefitsAsync(string chapa, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT DISTINCT
                LTRIM(RTRIM(F.CODEVENTO)) AS Code,
                COALESCE(LTRIM(RTRIM(E.DESCRICAO)), LTRIM(RTRIM(F.CODEVENTO))) AS Label,
                CASE
                    WHEN UPPER(E.DESCRICAO) LIKE '%REFEI%' OR UPPER(E.DESCRICAO) LIKE '%VR%' THEN 'Alimentacao'
                    WHEN UPPER(E.DESCRICAO) LIKE '%TRANSPORT%' OR UPPER(E.DESCRICAO) LIKE '%VT%' THEN 'Mobilidade'
                    WHEN UPPER(E.DESCRICAO) LIKE '%SAUDE%' OR UPPER(E.DESCRICAO) LIKE '%PLANO%' THEN 'Saude'
                    ELSE 'Beneficio'
                END AS Category,
                CONCAT('R$ ', FORMAT(F.VALOR, 'N2', 'pt-BR')) AS Value,
                'Ativo' AS Status,
                CONCAT('Evento folha ', LTRIM(RTRIM(F.CODEVENTO))) AS Details
            FROM dbo.PFFINANC F WITH (NOLOCK)
            INNER JOIN dbo.PEVENTO E WITH (NOLOCK)
                ON E.CODCOLIGADA = F.CODCOLIGADA AND E.CODIGO = F.CODEVENTO
            WHERE F.CODCOLIGADA = @CodColigada
              AND F.CHAPA = @Chapa
              AND (
                    UPPER(E.DESCRICAO) LIKE '%VALE%'
                 OR UPPER(E.DESCRICAO) LIKE '%VR%'
                 OR UPPER(E.DESCRICAO) LIKE '%VT%'
                 OR UPPER(E.DESCRICAO) LIKE '%PLANO%'
                 OR UPPER(E.DESCRICAO) LIKE '%SAUDE%'
                 OR UPPER(E.DESCRICAO) LIKE '%ODONTO%'
              )
              AND F.ANOCOMP = (
                    SELECT MAX(F2.ANOCOMP * 100 + F2.MESCOMP)
                    FROM dbo.PFFINANC F2 WITH (NOLOCK)
                    WHERE F2.CODCOLIGADA = F.CODCOLIGADA AND F2.CHAPA = F.CHAPA
              ) / 100
              AND F.MESCOMP = (
                    SELECT MAX(F2.ANOCOMP * 100 + F2.MESCOMP)
                    FROM dbo.PFFINANC F2 WITH (NOLOCK)
                    WHERE F2.CODCOLIGADA = F.CODCOLIGADA AND F2.CHAPA = F.CHAPA
              ) % 100;
            """;

        return _queryExecutor.QueryAsync(
            "benefits from PFFINANC",
            async (runtime, connection, token) =>
            {
                var rows = await connection.QueryAsync<RmBenefitRecord>(sql, new
                {
                    CodColigada = runtime.CodColigada,
                    Chapa = chapa
                });
                return (IReadOnlyList<RmBenefitRecord>)rows.ToList();
            },
            cancellationToken);
    }

    public Task<IReadOnlyList<RmDependentRecord>> GetDependentsAsync(string chapa, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                LTRIM(RTRIM(D.NOME)) AS Name,
                COALESCE(LTRIM(RTRIM(D.GRAUPARENTESCO)), 'Dependente') AS Relationship,
                CASE WHEN D.INATIVO = 1 THEN 'Inativo' ELSE 'Ativo' END AS Status
            FROM dbo.PFDEPEND D WITH (NOLOCK)
            INNER JOIN dbo.PFUNC F WITH (NOLOCK)
                ON F.CODCOLIGADA = D.CODCOLIGADA AND F.CODPESSOA = D.CODPESSOA
            WHERE F.CODCOLIGADA = @CodColigada
              AND F.CHAPA = @Chapa
            ORDER BY D.NOME;
            """;

        return _queryExecutor.TryQueryAsync(
            "PFDEPEND dependents",
            async (runtime, connection, token) =>
            {
                var rows = await connection.QueryAsync<RmDependentRecord>(sql, new
                {
                    CodColigada = runtime.CodColigada,
                    Chapa = chapa
                });
                return (IReadOnlyList<RmDependentRecord>?)rows.ToList();
            },
            cancellationToken,
            throwWhenDisabled: false)!
            .ContinueWith(
                task => (IReadOnlyList<RmDependentRecord>)(task.Result ?? []),
                cancellationToken);
    }
}
