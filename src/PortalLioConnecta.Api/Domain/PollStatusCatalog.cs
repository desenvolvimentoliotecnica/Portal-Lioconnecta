namespace PortalLioConnecta.Api.Domain;

public static class PollStatusCatalog
{
    public const string Draft = "Draft";
    public const string Published = "Published";
    public const string Closed = "Closed";
    public const string Archived = "Archived";

    private static readonly IReadOnlyDictionary<string, string> Definitions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [Draft] = "Rascunho",
        [Published] = "Publicada",
        [Closed] = "Encerrada",
        [Archived] = "Arquivada"
    };

    public static string Normalize(string? status)
        => Definitions.Keys.FirstOrDefault(item => string.Equals(item, status, StringComparison.OrdinalIgnoreCase))
            ?? Draft;

    public static string GetLabel(string? status)
        => Definitions.TryGetValue(Normalize(status), out var label)
            ? label
            : "Rascunho";

    public static IReadOnlyList<KeyValuePair<string, string>> GetAll()
        => Definitions.ToList();
}
