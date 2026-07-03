namespace PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

public sealed class RmBenefitRecord
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Status { get; set; } = "Ativo";
    public string Details { get; set; } = string.Empty;
}

public sealed class RmDependentRecord
{
    public string Name { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string Status { get; set; } = "Ativo";
}
