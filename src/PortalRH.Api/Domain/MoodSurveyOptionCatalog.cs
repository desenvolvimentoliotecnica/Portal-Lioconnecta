namespace PortalRH.Api.Domain;

public static class MoodSurveyOptionCatalog
{
    public const string Motivated = "motivated";
    public const string Good = "good";
    public const string Tired = "tired";

    public static readonly IReadOnlyList<MoodSurveyOptionDefinition> Options =
    [
        new(Motivated, "😄", "Motivado", "Que energia! Continue inspirando o time hoje."),
        new(Good, "🙂", "Bem", "Otimo! Um dia equilibrado comeca com uma boa atitude."),
        new(Tired, "😴", "Cansado", "Respire fundo. Cada passo conta — voce nao esta sozinho.")
    ];

    public static bool IsValid(string? optionKey)
    {
        return Options.Any(item => item.Key == optionKey);
    }

    public static MoodSurveyOptionDefinition? Find(string? optionKey)
    {
        return Options.FirstOrDefault(item => item.Key == optionKey);
    }
}

public sealed record MoodSurveyOptionDefinition(
    string Key,
    string Emoji,
    string Label,
    string ThankYouMessage);
