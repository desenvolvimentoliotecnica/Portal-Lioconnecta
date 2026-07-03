namespace PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

public sealed class RmPayslipSummaryRecord
{
    public int AnoComp { get; set; }
    public int MesComp { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal NetAmount { get; set; }
    public DateTime? PaymentDate { get; set; }
}

public sealed class RmPayslipLineRecord
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsDeduction { get; set; }
}
