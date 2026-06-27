namespace PortalRH.Api.Infrastructure;

public static class PortalUploadPaths
{
    public const string FeedFolderName = "feed";

    public static string ResolveUploadsRoot(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configured = configuration["FileStorage:UploadsRoot"]?.Trim();
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return Path.GetFullPath(configured);
        }

        if (!environment.IsDevelopment())
        {
            // Mantem uploads fora da pasta versionada da release (releases/<sha>/api).
            return Path.GetFullPath(Path.Combine(
                environment.ContentRootPath,
                "..",
                "..",
                "..",
                "shared",
                "uploads"));
        }

        var webRootPath = environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(environment.ContentRootPath, "wwwroot");
        }

        return Path.Combine(webRootPath, "uploads");
    }

    public static void EnsureFeedUploadsReady(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var uploadsRoot = ResolveUploadsRoot(configuration, environment);
        var feedDirectory = Path.Combine(uploadsRoot, FeedFolderName);
        Directory.CreateDirectory(feedDirectory);

        var webRootPath = environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(environment.ContentRootPath, "wwwroot");
        }

        var legacyFeedDirectory = Path.Combine(webRootPath, "uploads", FeedFolderName);
        if (!Directory.Exists(legacyFeedDirectory))
        {
            return;
        }

        if (string.Equals(
                Path.GetFullPath(legacyFeedDirectory),
                Path.GetFullPath(feedDirectory),
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        foreach (var filePath in Directory.EnumerateFiles(legacyFeedDirectory))
        {
            var destination = Path.Combine(feedDirectory, Path.GetFileName(filePath));
            if (!File.Exists(destination))
            {
                File.Copy(filePath, destination);
            }
        }
    }

    public static string BuildFeedPublicUrl(string fileName)
    {
        return $"/uploads/{FeedFolderName}/{fileName}";
    }
}
