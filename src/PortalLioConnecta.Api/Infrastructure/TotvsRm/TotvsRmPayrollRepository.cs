using Dapper;
using PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public class TotvsRmPayrollRepository : ITotvsRmPayrollRepository
{
    private readonly TotvsRmQueryExecutor _queryExecutor;

    public TotvsRmPayrollRepository(TotvsRmQueryExecutor queryExecutor)
    {
        _queryExecutor = queryExecutor;
    }

    public Task<IReadOnlyList<RmPayslipSummaryRecord>> GetPayslipSummariesAsync(
        string chapa,
        int maxMonths,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (@MaxMonths)
                F.ANOCOMP AS AnoComp,
                F.MESCOMP AS MesComp,
                SUM(CASE WHEN E.PROVDESCBASE = 'P' THEN F.VALOR ELSE 0 END) AS GrossAmount,
                SUM(CASE WHEN E.PROVDESCBASE = 'P' THEN F.VALOR ELSE 0 END)
                    - SUM(CASE WHEN E.PROVDESCBASE = 'D' THEN F.VALOR ELSE 0 END) AS NetAmount,
                MAX(F.DTPAGTO) AS PaymentDate
            FROM dbo.PFFINANC F WITH (NOLOCK)
            INNER JOIN dbo.PEVENTO E WITH (NOLOCK)
                ON E.CODCOLIGADA = F.CODCOLIGADA AND E.CODIGO = F.CODEVENTO
            WHERE F.CODCOLIGADA = @CodColigada
              AND F.CHAPA = @Chapa
            GROUP BY F.ANOCOMP, F.MESCOMP
            ORDER BY F.ANOCOMP DESC, F.MESCOMP DESC;
            """;

        return _queryExecutor.QueryAsync(
            "PFFINANC summaries",
            async (runtime, connection, token) =>
            {
                var rows = await connection.QueryAsync<RmPayslipSummaryRecord>(sql, new
                {
                    CodColigada = runtime.CodColigada,
                    Chapa = chapa,
                    MaxMonths = maxMonths
                });
                return (IReadOnlyList<RmPayslipSummaryRecord>)rows.ToList();
            },
            cancellationToken);
    }

    public Task<IReadOnlyList<RmPayslipLineRecord>> GetPayslipLinesAsync(
        string chapa,
        int anoComp,
        int mesComp,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                LTRIM(RTRIM(F.CODEVENTO)) AS Code,
                COALESCE(LTRIM(RTRIM(E.DESCRICAO)), LTRIM(RTRIM(F.CODEVENTO))) AS Description,
                COALESCE(LTRIM(RTRIM(CAST(F.REF AS VARCHAR(32)))), '—') AS Reference,
                F.VALOR AS Amount,
                CASE WHEN E.PROVDESCBASE = 'D' THEN 1 ELSE 0 END AS IsDeduction
            FROM dbo.PFFINANC F WITH (NOLOCK)
            LEFT JOIN dbo.PEVENTO E WITH (NOLOCK)
                ON E.CODCOLIGADA = F.CODCOLIGADA AND E.CODIGO = F.CODEVENTO
            WHERE F.CODCOLIGADA = @CodColigada
              AND F.CHAPA = @Chapa
              AND F.ANOCOMP = @AnoComp
              AND F.MESCOMP = @MesComp
              AND F.VALOR <> 0
            ORDER BY E.PROVDESCBASE, F.CODEVENTO;
            """;

        return _queryExecutor.QueryAsync(
            "PFFINANC lines",
            async (runtime, connection, token) =>
            {
                var rows = await connection.QueryAsync<RmPayslipLineRecord>(sql, new
                {
                    CodColigada = runtime.CodColigada,
                    Chapa = chapa,
                    AnoComp = anoComp,
                    MesComp = mesComp
                });
                return (IReadOnlyList<RmPayslipLineRecord>)rows.ToList();
            },
            cancellationToken);
    }
}
