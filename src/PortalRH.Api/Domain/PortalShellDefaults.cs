using System.Text.Json;
using PortalRH.Api.Contracts.Shell;

namespace PortalRH.Api.Domain;

public static class PortalShellDefaults
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Lazy<MeUiResponse> MeUiTemplate = new(LoadMeUiTemplate);
    private static readonly Lazy<PanelsResponse> PanelsTemplate = new(LoadPanelsTemplate);

    public static MeUiResponse CreateMeUiTemplate() => MeUiTemplate.Value;

    public static PanelsResponse CreatePanelsTemplate() => PanelsTemplate.Value;

    private static MeUiResponse LoadMeUiTemplate()
    {
        var json = ReadTemplate("me-ui.template.json");
        return JsonSerializer.Deserialize<MeUiResponse>(json, JsonOptions)
            ?? throw new InvalidOperationException("Template me-ui invalido.");
    }

    private static PanelsResponse LoadPanelsTemplate()
    {
        var json = ReadTemplate("panels.template.json");
        return JsonSerializer.Deserialize<PanelsResponse>(json, JsonOptions)
            ?? throw new InvalidOperationException("Template panels invalido.");
    }

    private static string ReadTemplate(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Defaults", fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Template de shell nao encontrado: {path}", path);
        }

        return File.ReadAllText(path);
    }
}
