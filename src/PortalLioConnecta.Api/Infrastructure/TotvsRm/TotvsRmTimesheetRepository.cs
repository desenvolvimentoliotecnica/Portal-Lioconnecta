using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Services;

namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public class TotvsRmTimesheetRepository : ITotvsRmTimesheetRepository
{
    private readonly TotvsRmQueryExecutor _queryExecutor;
    private readonly TotvsRmPeriodBankReader _periodBankReader;

    public TotvsRmTimesheetRepository(
        TotvsRmQueryExecutor queryExecutor,
        TotvsRmPeriodBankReader periodBankReader)
    {
        _queryExecutor = queryExecutor;
        _periodBankReader = periodBankReader;
    }

    public Task<IReadOnlyList<RmPunchRecord>> GetPunchesAsync(
        string chapa,
        DateTime dataDe,
        DateTime dataAte,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                CAST(B.DATA AS DATE)            AS DataPonto,
                B.BATIDA                        AS BatidaMinutos,
                B.NATUREZA                      AS Natureza,
                NAT.DESCRICAO                   AS DescricaoNatureza,
                B.STATUS                        AS Status,
                CAST(B.CODIGOTERMCOL AS VARCHAR(32)) AS CodigoRelogio
            FROM dbo.ABATFUN B WITH (NOLOCK)
            LEFT JOIN dbo.ANATUBAT NAT WITH (NOLOCK)
                ON NAT.CODINTERNO = B.NATUREZA
            WHERE B.CODCOLIGADA = @CodColigada
              AND B.CHAPA       = @Chapa
              AND CAST(B.DATA AS DATE) BETWEEN @DataDe AND @DataAte
            ORDER BY B.DATA, B.BATIDA;
            """;

        return _queryExecutor.QueryAsync(
            TotvsRmConstants.PunchTableName,
            async (runtime, connection, token) =>
            {
                var rows = await connection.QueryAsync<RmPunchRecord>(sql, new
                {
                    CodColigada = runtime.CodColigada,
                    Chapa = chapa,
                    DataDe = dataDe.Date,
                    DataAte = dataAte.Date
                });
                return (IReadOnlyList<RmPunchRecord>)rows.ToList();
            },
            cancellationToken);
    }

    public Task<IReadOnlyList<RmProcessedDayRecord>> GetProcessedDaysAsync(
        string chapa,
        DateTime dataDe,
        DateTime dataAte,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                CAST(H.DATA AS DATE) AS DataPonto,
                H.HTRAB              AS WorkedMinutes,
                COALESCE(H.BASE, H.TEMPOREF) AS ExpectedMinutes,
                CASE
                    WHEN COALESCE(H.BASE, H.TEMPOREF) IS NOT NULL
                        THEN COALESCE(H.HTRAB, 0)
                           + COALESCE(H.ABONO, 0)
                           + COALESCE(H.COMPENSADO, 0)
                           - COALESCE(H.BASE, H.TEMPOREF, 0)
                    ELSE NULL
                END AS BalanceMinutes,
                COALESCE(H.ATRASOCALC, H.ATRASO) AS DelayMinutes,
                COALESCE(H.FALTACALC, H.FALTA) AS AbsenceMinutes,
                H.ABONO              AS AbonoMinutes,
                H.EXTRAAUTORIZADO    AS AuthorizedOvertimeMinutes,
                H.COMPENSADO         AS CompensatedMinutes,
                CASE
                    WHEN COALESCE(H.ABONO, 0) > 0 THEN 'B'
                    WHEN COALESCE(H.EXTRAAUTORIZADO, 0) > 0 THEN 'E'
                    WHEN COALESCE(H.FALTACALC, H.FALTA, 0) > 0 THEN 'F'
                    WHEN COALESCE(H.ATRASOCALC, H.ATRASO, 0) > 0 THEN 'A'
                    ELSE 'D'
                END AS StatusCode
            FROM dbo.AAFHTFUN H WITH (NOLOCK)
            WHERE H.CODCOLIGADA = @CodColigada
              AND H.CHAPA       = @Chapa
              AND CAST(H.DATA AS DATE) BETWEEN @DataDe AND @DataAte
            ORDER BY H.DATA;
            """;

        return _queryExecutor.QueryAsync(
            TotvsRmConstants.ProcessedDayTableName,
            async (runtime, connection, token) =>
            {
                var rows = await connection.QueryAsync<RmProcessedDayRecord>(sql, new
                {
                    CodColigada = runtime.CodColigada,
                    Chapa = chapa,
                    DataDe = dataDe.Date,
                    DataAte = dataAte.Date
                });
                return (IReadOnlyList<RmProcessedDayRecord>)rows.ToList();
            },
            cancellationToken);
    }

    public Task<RmPeriodBankSummary?> GetPeriodBankSummaryAsync(
        string chapa,
        DateTime dataDe,
        DateTime dataAte,
        CancellationToken cancellationToken)
    {
        return _queryExecutor.TryQueryAsync(
            "ASALDOBANCOHOR",
            async (runtime, connection, token) =>
                await _periodBankReader.ReadAsync(
                    connection,
                    runtime.CodColigada,
                    chapa,
                    dataDe,
                    dataAte,
                    token),
            cancellationToken,
            throwWhenDisabled: true);
    }
}
