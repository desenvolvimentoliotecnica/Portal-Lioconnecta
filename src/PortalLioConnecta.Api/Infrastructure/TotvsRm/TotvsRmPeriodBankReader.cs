using Dapper;
using Microsoft.Data.SqlClient;
using PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public sealed class TotvsRmPeriodBankReader
{
    private static readonly string[] PreviousBalanceColumns =
    [
        "SALDOANTERIOR", "SALDOANT", "SALDOANTER", "SALDOANTERIORBH", "EXTRAANT"
    ];

    private static readonly string[] PeriodBalanceColumns =
    [
        "SALDOPERIODO", "SALDOPER", "SALDOATUALPER", "SALDOPERIODOBH", "EXTRAATU"
    ];

    private static readonly string[] TotalBalanceColumns =
    [
        "SALDOATUAL", "SALDOFINAL", "SALDOTOTAL", "SALDOACUMULADO", "SALDO"
    ];

    private static readonly string[] PeriodStartColumns =
    [
        "INICIOPER", "INICIOPERMES", "INICIOMENSAL", "DATAINICIO", "DATINICIO", "INICIO", "DATAINI"
    ];

    private static readonly string[] PeriodEndColumns =
    [
        "FIMPER", "FIMPERMES", "FIMMENSAL", "DATAFIM", "DATFIM", "FIM"
    ];

    public async Task<RmPeriodBankSummary?> ReadAsync(
        SqlConnection connection,
        short codColigada,
        string chapa,
        DateTime dataDe,
        DateTime dataAte,
        CancellationToken cancellationToken)
    {
        var asaldo = await TryReadFromAsaldoBancoHorAsync(
            connection,
            codColigada,
            chapa,
            dataDe,
            dataAte,
            cancellationToken);
        if (asaldo is not null)
        {
            return asaldo;
        }

        foreach (var tableName in new[] { "ACOMPFUN", "ABANCOHORFUNDETALHE" })
        {
            var summary = await TryReadFromTableAsync(
                connection,
                tableName,
                codColigada,
                chapa,
                dataDe,
                dataAte,
                cancellationToken);

            if (summary is not null)
            {
                return summary;
            }
        }

        return null;
    }

    public static int? CalculateNetBalance(int? extra, int? delay, int? absence)
    {
        if (!extra.HasValue && !delay.HasValue && !absence.HasValue)
        {
            return null;
        }

        return (extra ?? 0) - (delay ?? 0) - (absence ?? 0);
    }

    private static async Task<RmPeriodBankSummary?> TryReadFromAsaldoBancoHorAsync(
        SqlConnection connection,
        short codColigada,
        string chapa,
        DateTime dataDe,
        DateTime dataAte,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1
                EXTRAANT AS ExtraAnt,
                ATRASOANT AS AtrasoAnt,
                FALTAANT AS FaltaAnt,
                EXTRAATU AS ExtraAtu,
                ATRASOATU AS AtrasoAtu,
                FALTAATU AS FaltaAtu
            FROM dbo.ASALDOBANCOHOR WITH (NOLOCK)
            WHERE CODCOLIGADA = @CodColigada
              AND CHAPA = @Chapa
              AND TRY_CONVERT(date, INICIOPER) = @DataDe
              AND TRY_CONVERT(date, FIMPER) = @DataAte
            ORDER BY FIMPER DESC;
            """;

        try
        {
            var row = await connection.QueryFirstOrDefaultAsync<AsaldoBancoHorRow>(
                new CommandDefinition(sql, new
                {
                    CodColigada = codColigada,
                    Chapa = chapa,
                    DataDe = dataDe.Date,
                    DataAte = dataAte.Date
                }, cancellationToken: cancellationToken));

            if (row is null)
            {
                return null;
            }

            var previous = CalculateNetBalance(row.ExtraAnt, row.AtrasoAnt, row.FaltaAnt);
            var period = CalculateNetBalance(row.ExtraAtu, row.AtrasoAtu, row.FaltaAtu);

            if (!previous.HasValue && !period.HasValue)
            {
                return null;
            }

            var total = previous.HasValue && period.HasValue
                ? previous.Value + period.Value
                : previous ?? period;

            return new RmPeriodBankSummary
            {
                PreviousBalanceMinutes = previous,
                PeriodBalanceMinutes = period,
                TotalBalanceMinutes = total,
                SourceTable = "ASALDOBANCOHOR"
            };
        }
        catch
        {
            return null;
        }
    }

    private sealed class AsaldoBancoHorRow
    {
        public int? ExtraAnt { get; init; }
        public int? AtrasoAnt { get; init; }
        public int? FaltaAnt { get; init; }
        public int? ExtraAtu { get; init; }
        public int? AtrasoAtu { get; init; }
        public int? FaltaAtu { get; init; }
    }

    private static async Task<RmPeriodBankSummary?> TryReadFromTableAsync(
        SqlConnection connection,
        string tableName,
        short codColigada,
        string chapa,
        DateTime dataDe,
        DateTime dataAte,
        CancellationToken cancellationToken)
    {
        var columns = await GetColumnNamesAsync(connection, tableName, cancellationToken);
        if (columns.Count == 0 || !columns.Contains("CHAPA", StringComparer.OrdinalIgnoreCase))
        {
            return null;
        }

        var previousColumn = FirstExisting(columns, PreviousBalanceColumns);
        var periodColumn = FirstExisting(columns, PeriodBalanceColumns);
        var totalColumn = FirstExisting(columns, TotalBalanceColumns);
        var startColumn = FirstExisting(columns, PeriodStartColumns);
        var endColumn = FirstExisting(columns, PeriodEndColumns);

        if (previousColumn is null && periodColumn is null && totalColumn is null)
        {
            return null;
        }

        var selectParts = new List<string>();
        if (previousColumn is not null)
        {
            selectParts.Add($"TRY_CONVERT(INT, [{previousColumn}]) AS PreviousBalanceMinutes");
        }

        if (periodColumn is not null)
        {
            selectParts.Add($"TRY_CONVERT(INT, [{periodColumn}]) AS PeriodBalanceMinutes");
        }

        if (totalColumn is not null)
        {
            selectParts.Add($"TRY_CONVERT(INT, [{totalColumn}]) AS TotalBalanceMinutes");
        }

        var whereParts = new List<string>
        {
            "CODCOLIGADA = @CodColigada",
            "CHAPA = @Chapa"
        };

        if (startColumn is not null && endColumn is not null)
        {
            whereParts.Add(
                $"TRY_CONVERT(date, [{startColumn}]) = @DataDe AND TRY_CONVERT(date, [{endColumn}]) = @DataAte");
        }
        else if (endColumn is not null)
        {
            whereParts.Add($"TRY_CONVERT(date, [{endColumn}]) BETWEEN @DataDe AND @DataAte");
        }

        var orderBy = endColumn is not null
            ? $"ORDER BY TRY_CONVERT(date, [{endColumn}]) DESC"
            : string.Empty;

        var sql = $"""
            SELECT TOP 1
                {string.Join(",\n                ", selectParts)}
            FROM dbo.[{tableName}] WITH (NOLOCK)
            WHERE {string.Join(" AND ", whereParts)}
            {orderBy};
            """;

        try
        {
            var row = await connection.QueryFirstOrDefaultAsync<dynamic>(
                new CommandDefinition(sql, new
                {
                    CodColigada = codColigada,
                    Chapa = chapa,
                    DataDe = dataDe.Date,
                    DataAte = dataAte.Date
                }, cancellationToken: cancellationToken));

            if (row is null)
            {
                return null;
            }

            var dict = (IDictionary<string, object?>)row;
            int? previous = ReadInt(dict, "PreviousBalanceMinutes");
            int? period = ReadInt(dict, "PeriodBalanceMinutes");
            int? total = ReadInt(dict, "TotalBalanceMinutes");

            if (total is null && previous.HasValue && period.HasValue)
            {
                total = previous.Value + period.Value;
            }

            if (!previous.HasValue && !period.HasValue && !total.HasValue)
            {
                return null;
            }

            return new RmPeriodBankSummary
            {
                PreviousBalanceMinutes = previous,
                PeriodBalanceMinutes = period,
                TotalBalanceMinutes = total,
                SourceTable = tableName
            };
        }
        catch
        {
            return null;
        }
    }

    private static async Task<HashSet<string>> GetColumnNamesAsync(
        SqlConnection connection,
        string tableName,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = 'dbo'
              AND TABLE_NAME = @TableName;
            """;

        var rows = await connection.QueryAsync<string>(
            new CommandDefinition(sql, new { TableName = tableName }, cancellationToken: cancellationToken));

        return rows.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static string? FirstExisting(HashSet<string> columns, IEnumerable<string> candidates)
    {
        foreach (var candidate in candidates)
        {
            if (columns.Contains(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static int? ReadInt(IDictionary<string, object?> row, string key)
    {
        if (!row.TryGetValue(key, out var value) || value is null or DBNull)
        {
            return null;
        }

        return value switch
        {
            int intValue => intValue,
            short shortValue => shortValue,
            long longValue => (int)longValue,
            decimal decimalValue => (int)decimalValue,
            double doubleValue => (int)doubleValue,
            _ => int.TryParse(value.ToString(), out var parsed) ? parsed : null
        };
    }
}
