using PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

namespace PortalLioConnecta.Api.Services;

public static class HrRmMapper
{
    private static readonly string[] MonthNames =
    [
        "", "Janeiro", "Fevereiro", "Marco", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    ];

    private static readonly string[] ShortMonthNames =
    [
        "", "JAN.", "FEV.", "MAR.", "ABR.", "MAI.", "JUN.",
        "JUL.", "AGO.", "SET.", "OUT.", "NOV.", "DEZ."
    ];

    public static string BuildPeriodId(int anoComp, int mesComp) =>
        $"{anoComp:D4}-{mesComp:D2}";

    public static string BuildPayslipId(int anoComp, int mesComp, int nroPeriodo, string paymentType = "FOLHA")
    {
        if (string.Equals(paymentType, "ADIANTAMENTO", StringComparison.OrdinalIgnoreCase) && nroPeriodo <= 1)
        {
            return $"{anoComp:D4}-{mesComp:D2}-ADIANTAMENTO";
        }

        return nroPeriodo <= 1
            ? BuildPeriodId(anoComp, mesComp)
            : $"{anoComp:D4}-{mesComp:D2}-{nroPeriodo}";
    }

    public static bool TryParsePayslipId(
        string payslipId,
        out int anoComp,
        out int mesComp,
        out int? nroPeriodo,
        out string? paymentTypeHint)
    {
        anoComp = 0;
        mesComp = 0;
        nroPeriodo = null;
        paymentTypeHint = null;

        var parts = payslipId.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2 ||
            !int.TryParse(parts[0], out anoComp) ||
            !int.TryParse(parts[1], out mesComp) ||
            mesComp is < 1 or > 12)
        {
            return false;
        }

        if (parts.Length >= 3)
        {
            if (parts[2].Equals("ADIANTAMENTO", StringComparison.OrdinalIgnoreCase))
            {
                paymentTypeHint = "ADIANTAMENTO";
                return true;
            }

            if (int.TryParse(parts[2], out var parsedPeriod))
            {
                nroPeriodo = parsedPeriod;
            }
        }

        return true;
    }

    public static bool TryParsePayslipId(string payslipId, out int anoComp, out int mesComp, out int? nroPeriodo) =>
        TryParsePayslipId(payslipId, out anoComp, out mesComp, out nroPeriodo, out _);

    public static int ResolveNroPeriodo(
        IReadOnlyList<RmPayslipSummaryRecord> envelopes,
        int? explicitNroPeriodo,
        string? paymentTypeHint = null)
    {
        if (!string.IsNullOrWhiteSpace(paymentTypeHint))
        {
            var typedEnvelope = envelopes.FirstOrDefault(item =>
                string.Equals(MapPaymentTypeLabel(item), paymentTypeHint, StringComparison.OrdinalIgnoreCase));
            if (typedEnvelope is not null)
            {
                return typedEnvelope.NroPeriodo;
            }
        }

        if (explicitNroPeriodo.HasValue)
        {
            return explicitNroPeriodo.Value;
        }

        if (envelopes.Count == 0)
        {
            return 1;
        }

        if (envelopes.Count == 1)
        {
            return envelopes[0].NroPeriodo;
        }

        var folha = envelopes.FirstOrDefault(item => MapPaymentTypeLabel(item) == "FOLHA");
        return folha?.NroPeriodo ?? envelopes[0].NroPeriodo;
    }

    public static string MapPaymentTypeLabel(RmPayslipSummaryRecord summary)
    {
        if (summary.HasAdvanceEvent && !summary.HasPayrollEvents)
        {
            return "ADIANTAMENTO";
        }

        if (summary.NroPeriodo > 1 && !summary.HasPayrollEvents)
        {
            return "ADIANTAMENTO";
        }

        if (summary.DeductionAmount <= 0.01m &&
            summary.GrossAmount > 0m &&
            summary.NetAmount >= summary.GrossAmount - 0.01m)
        {
            return "ADIANTAMENTO";
        }

        if (summary.PaymentDate?.Day is int paymentDay &&
            paymentDay <= 20 &&
            summary.DeductionAmount <= 0.01m)
        {
            return "ADIANTAMENTO";
        }

        return "FOLHA";
    }

    public static string BuildPaymentTypeTitle(string paymentType) =>
        paymentType == "ADIANTAMENTO"
            ? "Pagamento em ADIANTAMENTO"
            : "Pagamento em FOLHA";

    public static string BuildCompetenceTitle(int anoComp, int mesComp) =>
        mesComp is >= 1 and <= 12
            ? $"{MonthNames[mesComp]} {anoComp}"
            : $"{mesComp:D2}/{anoComp}";

    public static string BuildShortMonthLabel(int mesComp) =>
        mesComp is >= 1 and <= 12 ? ShortMonthNames[mesComp] : $"{mesComp:D2}";

    public static string BuildPeriodLabel(int anoComp, int mesComp) =>
        mesComp is >= 1 and <= 12
            ? $"{MonthNames[mesComp]}/{anoComp}"
            : $"{mesComp:D2}/{anoComp}";

    public static string MapVacationStatus(string statusCode) => statusCode?.Trim().ToUpperInvariant() switch
    {
        "M" or "G" => "Gozado",
        "P" or "A" => "Programado",
        "F" => "Aprovado",
        "C" => "Cancelado",
        _ => "Em analise"
    };

    public static string MaskCpf(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
        {
            return "—";
        }

        var digits = new string(cpf.Where(char.IsDigit).ToArray());
        if (digits.Length != 11)
        {
            return cpf;
        }

        return $"***.{digits.Substring(3, 3)}.{digits.Substring(6, 3)}-**";
    }

    public static string FormatCityState(string? city, string? state)
    {
        var parts = new[] { city?.Trim(), state?.Trim() }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToArray();
        return parts.Length == 0 ? "—" : string.Join(" / ", parts);
    }

    public static string FormatAdmissionDate(DateTime? date) =>
        date?.ToString("dd/MM/yyyy") ?? "—";

    public static string FormatPaymentDateShort(DateTime? date) =>
        date?.ToString("dd/MM") ?? "—";
}
