using System.Text.Json.Nodes;

namespace PortalLioConnecta.Api.Contracts.Shell;

public sealed record PanelDto(
    string Type,
    string Title,
    string Name,
    string Subtitle,
    string Description,
    string Manager,
    string ModuleKey,
    IReadOnlyList<JsonNode>? Items);

public sealed record PanelsResponse(
    IReadOnlyList<PanelDto> LeftPanels,
    IReadOnlyList<PanelDto> RightPanels);
