namespace PortalLioConnecta.Api.Domain;

public static class PollResultsVisibilityCatalog
{
    public const string Always = "Always";
    public const string AfterVote = "AfterVote";
    public const string AfterClose = "AfterClose";

    private static readonly IReadOnlyDictionary<string, string> Definitions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [Always] = "Sempre exibir",
        [AfterVote] = "Exibir apos voto",
        [AfterClose] = "Exibir apos encerramento"
    };

    public static string Normalize(string? value)
        => Definitions.Keys.FirstOrDefault(item => string.Equals(item, value, StringComparison.OrdinalIgnoreCase))
            ?? AfterVote;

    public static string GetLabel(string? value)
        => Definitions.TryGetValue(Normalize(value), out var label)
            ? label
            : "Exibir apos voto";

    public static IReadOnlyList<KeyValuePair<string, string>> GetAll()
        => Definitions.ToList();
}
