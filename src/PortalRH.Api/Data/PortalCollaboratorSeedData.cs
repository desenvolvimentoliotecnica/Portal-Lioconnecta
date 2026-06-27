namespace PortalRH.Api.Data;

public static class PortalCollaboratorSeedData
{
    public const string DefaultPassword = "Liotec@2026";

    public static readonly IReadOnlyList<PortalCollaboratorSeedEntry> Entries =
    [
        new(
            Guid.Parse("8f4b2f6e-1c2d-4a5b-9e8f-111111111101"),
            "colaborador1@liotecnica.com.br",
            "Colaborador Um",
            "Sistemas"),
        new(
            Guid.Parse("8f4b2f6e-1c2d-4a5b-9e8f-111111111102"),
            "colaborador2@liotecnica.com.br",
            "Colaborador Dois",
            "Operacoes"),
        new(
            Guid.Parse("8f4b2f6e-1c2d-4a5b-9e8f-111111111103"),
            "colaborador3@liotecnica.com.br",
            "Colaborador Tres",
            "Administrativo")
    ];
}

public sealed record PortalCollaboratorSeedEntry(
    Guid Id,
    string Login,
    string DisplayName,
    string Department);
