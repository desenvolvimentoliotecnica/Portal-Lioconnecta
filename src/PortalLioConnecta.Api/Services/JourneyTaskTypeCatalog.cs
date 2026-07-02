namespace PortalLioConnecta.Api.Services;

internal sealed record JourneyTaskTypeDefinition(
    string Key,
    string ListTypeLabel,
    string DefaultStatus);

internal static class JourneyTaskTypeCatalog
{
    private static readonly IReadOnlyDictionary<string, JourneyTaskTypeDefinition> Types =
        new Dictionary<string, JourneyTaskTypeDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["rh-documento"] = new("rh-documento", "Documento RH", "Pendente"),
            ["rh-cadastro"] = new("rh-cadastro", "Cadastro RH", "Pendente"),
            ["treinamento-curso"] = new("treinamento-curso", "Curso / trilha", "Em andamento"),
            ["treinamento-certificacao"] = new("treinamento-certificacao", "Certificacao", "Pendente"),
            ["compliance-politica"] = new("compliance-politica", "Politica / compliance", "Pendente"),
            ["compliance-seguranca"] = new("compliance-seguranca", "Seguranca da informacao", "Em andamento"),
            ["ti-equipamento"] = new("ti-equipamento", "Equipamento TI", "Pendente"),
            ["ti-acesso"] = new("ti-acesso", "Acesso a sistema", "Pendente"),
            ["operacional-reuniao"] = new("operacional-reuniao", "Reuniao / compromisso", "Pendente"),
            ["operacional-entrega"] = new("operacional-entrega", "Entrega / projeto", "Em andamento"),
            ["engajamento-pesquisa"] = new("engajamento-pesquisa", "Pesquisa / enquete", "Pendente"),
            ["geral-outros"] = new("geral-outros", "Outros", "Pendente")
        };

    public static bool TryGet(string typeKey, out JourneyTaskTypeDefinition definition)
        => Types.TryGetValue(typeKey, out definition!);

    public static IReadOnlyCollection<string> Keys => Types.Keys.ToArray();
}
