namespace PortalLioConnecta.Api.Services;

internal sealed record JourneyRequestTypeDefinition(
    string Key,
    string ListTypeLabel,
    string DefaultStage,
    string DefaultStatus);

internal static class JourneyRequestTypeCatalog
{
    private static readonly IReadOnlyDictionary<string, JourneyRequestTypeDefinition> Types =
        new Dictionary<string, JourneyRequestTypeDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["rh-ferias"] = new("rh-ferias", "Ferias", "Aprovacao do gestor", "Em analise"),
            ["rh-reembolso"] = new("rh-reembolso", "Reembolso", "Validacao financeira", "Em andamento"),
            ["rh-atestado"] = new("rh-atestado", "Atestado medico", "Conferencia RH", "Aguardando"),
            ["ti-chamado"] = new("ti-chamado", "Chamado TI", "Triagem de TI", "Aguardando"),
            ["ti-equipamento"] = new("ti-equipamento", "Equipamento", "Triagem de TI", "Aguardando"),
            ["ti-acesso"] = new("ti-acesso", "Acesso a sistema", "Aprovacao do gestor", "Em analise"),
            ["financeiro-nf"] = new("financeiro-nf", "Nota fiscal", "Validacao financeira", "Em andamento"),
            ["facilities-manutencao"] = new("facilities-manutencao", "Manutencao", "Triagem Facilities", "Aguardando"),
            ["geral-outros"] = new("geral-outros", "Outros", "Triagem inicial", "Aguardando")
        };

    public static bool TryGet(string typeKey, out JourneyRequestTypeDefinition definition)
        => Types.TryGetValue(typeKey, out definition!);

    public static IReadOnlyCollection<string> Keys => Types.Keys.ToArray();
}
