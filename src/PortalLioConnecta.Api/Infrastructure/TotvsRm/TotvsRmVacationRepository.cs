using Dapper;
using PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public class TotvsRmVacationRepository : ITotvsRmVacationRepository
{
    private readonly TotvsRmQueryExecutor _queryExecutor;

    public TotvsRmVacationRepository(TotvsRmQueryExecutor queryExecutor)
    {
        _queryExecutor = queryExecutor;
    }

    public Task<RmVacationBalanceRecord?> GetBalanceAsync(string chapa, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1
                CAST(COALESCE(V.SALDO, 0) AS INT) AS AvailableDays,
                CAST(COALESCE(V.FALTAS, 0) AS INT) AS ScheduledDays,
                CAST(COALESCE(V.DIASGOZADOS, 0) AS INT) AS UsedDays,
                V.FIMPERAQUIS AS NextAcquisitionDate
            FROM dbo.PFUFERIAS V WITH (NOLOCK)
            WHERE V.CODCOLIGADA = @CodColigada
              AND V.CHAPA = @Chapa
            ORDER BY V.FIMPERAQUIS DESC;
            """;

        return _queryExecutor.TryQueryAsync(
            "PFUFERIAS balance",
            async (runtime, connection, token) =>
            {
                return await connection.QueryFirstOrDefaultAsync<RmVacationBalanceRecord>(sql, new
                {
                    CodColigada = runtime.CodColigada,
                    Chapa = chapa
                });
            },
            cancellationToken,
            throwWhenDisabled: false);
    }

    public Task<IReadOnlyList<RmVacationPeriodRecord>> GetPeriodsAsync(string chapa, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                P.DATAINICIO AS StartDate,
                P.DATAFIM AS EndDate,
                CAST(COALESCE(P.NRODIASFERIAS, 0) AS INT) AS Days,
                COALESCE(LTRIM(RTRIM(P.SITUACAOFERIAS)), 'P') AS StatusCode,
                P.DATAAVISO AS RequestedAt
            FROM dbo.PFUFERIASPER P WITH (NOLOCK)
            WHERE P.CODCOLIGADA = @CodColigada
              AND P.CHAPA = @Chapa
            ORDER BY P.DATAINICIO DESC;
            """;

        return _queryExecutor.QueryAsync(
            "PFUFERIASPER periods",
            async (runtime, connection, token) =>
            {
                var rows = await connection.QueryAsync<RmVacationPeriodRecord>(sql, new
                {
                    CodColigada = runtime.CodColigada,
                    Chapa = chapa
                });
                return (IReadOnlyList<RmVacationPeriodRecord>)rows.ToList();
            },
            cancellationToken);
    }
}
