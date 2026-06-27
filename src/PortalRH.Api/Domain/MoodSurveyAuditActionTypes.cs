namespace PortalRH.Api.Domain;

public static class MoodSurveyAuditActionTypes
{
    public const string VoteSubmitted = "HumorRegistrado";

    public static string GetLabel(string? actionType)
    {
        return actionType switch
        {
            VoteSubmitted => "Humor registrado",
            _ => actionType ?? "Evento"
        };
    }
}
