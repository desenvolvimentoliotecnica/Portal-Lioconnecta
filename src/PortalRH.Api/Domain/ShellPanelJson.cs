using System.Text.Json;
using System.Text.Json.Nodes;
using PortalRH.Api.Contracts.Agenda;

namespace PortalRH.Api.Domain;

internal static class ShellPanelJson
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static JsonNode LabelBadge(string label, string badge) =>
        Serialize(new { label, badge });

    public static JsonNode LabelValue(string label, string value) =>
        Serialize(new { label, value });

    public static JsonNode LabelOnly(string label, string? url = null) =>
        url is null ? Serialize(new { label }) : Serialize(new { label, url });

    public static JsonNode LabelDescription(string label, string? description = null) =>
        string.IsNullOrWhiteSpace(description)
            ? Serialize(new { label })
            : Serialize(new { label, description });

    public static JsonNode AgendaPanelItem(
        Guid id,
        string title,
        string timeLabel,
        string? description,
        string? location,
        string source,
        DateTime startAtUtc,
        DateTime endAtUtc,
        string? joinUrl = null,
        IReadOnlyList<AgendaParticipantDto>? participants = null) =>
        Serialize(new
        {
            type = "agenda-event",
            id = id.ToString(),
            title,
            timeLabel,
            label = $"{timeLabel} • {title}",
            description = location ?? description ?? string.Empty,
            detailDescription = description ?? string.Empty,
            location = location ?? string.Empty,
            source,
            joinUrl = joinUrl ?? string.Empty,
            participants = (participants ?? Array.Empty<AgendaParticipantDto>())
                .Select(participant => new
                {
                    name = participant.Name,
                    email = participant.Email,
                    role = participant.Role,
                    responseStatus = participant.ResponseStatus,
                    photoUrl = participant.PhotoUrl
                }),
            startAtUtc = startAtUtc.ToUniversalTime().ToString("O"),
            endAtUtc = endAtUtc.ToUniversalTime().ToString("O")
        });

    public static JsonNode LabelLink(string label, string url, string? badge = null) =>
        badge is null ? Serialize(new { label, url }) : Serialize(new { label, url, badge });

    public static JsonNode QuickLink(string className, string label, string shortLabel, string url) =>
        Serialize(new { className, label, shortLabel, url });

    public static JsonNode HrLink(string label, string url, string provider) =>
        Serialize(new { label, url, provider });

    private static JsonNode Serialize(object value) =>
        JsonSerializer.SerializeToNode(value, JsonOptions)!;
}
