using PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

namespace PortalLioConnecta.Api.Services;

public static class HrRmMapper
{
    private static readonly string[] MonthNames =
    [
        "", "Janeiro", "Fevereiro", "Marco", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    ];

    public static string BuildPeriodId(int anoComp, int mesComp) =>
        $"{anoComp:D4}-{mesComp:D2}";

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
}
