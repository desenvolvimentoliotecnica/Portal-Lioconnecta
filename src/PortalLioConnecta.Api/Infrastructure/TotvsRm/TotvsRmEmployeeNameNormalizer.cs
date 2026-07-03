using System.Globalization;
using System.Text;

namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public static class TotvsRmEmployeeNameNormalizer
{
    public static string? NormalizeForComparison(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return null;
        }

        var collapsed = string.Join(
            ' ',
            fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        if (string.IsNullOrWhiteSpace(collapsed))
        {
            return null;
        }

        return RemoveDiacritics(collapsed).ToUpperInvariant();
    }

    private static string RemoveDiacritics(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
