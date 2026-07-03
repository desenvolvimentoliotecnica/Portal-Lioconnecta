namespace PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

public sealed class RmVacationBalanceRecord
{
    public int AvailableDays { get; set; }
    public int ScheduledDays { get; set; }
    public int UsedDays { get; set; }
    public DateTime? NextAcquisitionDate { get; set; }
}

public sealed class RmVacationPeriodRecord
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Days { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public DateTime? RequestedAt { get; set; }
}
