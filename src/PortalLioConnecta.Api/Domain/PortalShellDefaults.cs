using System.Text.Json;
using PortalLioConnecta.Api.Contracts.Shell;

namespace PortalLioConnecta.Api.Domain;

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
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }

        var resourceName = $"{typeof(PortalShellDefaults).Assembly.GetName().Name}.Defaults.{fileName}";
        var stream = typeof(PortalShellDefaults).Assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new FileNotFoundException(
                $"Template de shell nao encontrado em disco nem como recurso embutido: {fileName}",
                path);
        }

        using (stream)
        using (var reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }
}
