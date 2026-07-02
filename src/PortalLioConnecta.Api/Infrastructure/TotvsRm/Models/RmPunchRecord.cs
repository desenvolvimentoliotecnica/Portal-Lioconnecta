namespace PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

public sealed class RmPunchRecord
{
    public DateTime DataPonto { get; init; }
    public int BatidaMinutos { get; init; }
    public int Natureza { get; init; }
    public string? DescricaoNatureza { get; init; }
    public string? Status { get; init; }
    public string? CodigoRelogio { get; init; }
}
