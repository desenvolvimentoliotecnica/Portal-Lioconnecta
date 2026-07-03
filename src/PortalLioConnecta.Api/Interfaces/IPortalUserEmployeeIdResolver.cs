using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Interfaces;

public interface IPortalUserEmployeeIdResolver
{
    Task<PortalUserEmployeeIdResolution> ResolveAsync(
        PortalUser user,
        bool persistWhenFound,
        CancellationToken cancellationToken);
}

public sealed class PortalUserEmployeeIdResolution
{
    public string? EmployeeId { get; init; }
    public string Source { get; init; } = "none";

    public string MessageLabel =>
        string.IsNullOrWhiteSpace(EmployeeId) ? "nao informada" : EmployeeId.Trim();

    public static string BuildMissingProfileMessage(string? employeeId) =>
        $"Sua matricula ({(string.IsNullOrWhiteSpace(employeeId) ? "nao informada" : employeeId.Trim())}) nao esta vinculada ao perfil. Solicite ao RH a regularizacao do cadastro.";
}
