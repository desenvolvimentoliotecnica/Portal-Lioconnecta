namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public static class TotvsRmChapaNormalizer
{
    public static string? Normalize(string? employeeId)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
        {
            return null;
        }

        return employeeId.Trim().PadLeft(TotvsRmConstants.ChapaLength, '0');
    }
}
