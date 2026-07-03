namespace PortalLioConnecta.Api.Infrastructure.TotvsRm.Models;

public sealed class RmTeamMemberRecord
{
    public string Chapa { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Status { get; set; } = "Ativo";
}
