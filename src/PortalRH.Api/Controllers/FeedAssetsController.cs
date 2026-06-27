using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using PortalRH.Api.Contracts.Feed;
using PortalRH.Api.Domain;
using PortalRH.Api.Infrastructure;
using PortalRH.Api.Security;

namespace PortalRH.Api.Controllers;

[ApiController]
[Route("api/feed/assets")]
[RequirePortalSession]
public class FeedAssetsController : ControllerBase
{
    private static readonly HashSet<string> AllowedImageExtensions =
    [
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    ];

    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public FeedAssetsController(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [ProducesResponseType(typeof(FeedAssetUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        if (!PortalModuleAccess.HasModuleAccess(
                session.PortalUser,
                PortalModulePermissionCatalog.Feed,
                PortalModulePermissionCatalog.Interact,
                PortalModulePermissionCatalog.Manage))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Voce nao possui permissao para publicar fotos no feed."
            });
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Selecione uma imagem para enviar." });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Formato de imagem nao suportado. Use JPG, PNG, WEBP ou GIF." });
        }

        var uploadsRoot = PortalUploadPaths.ResolveUploadsRoot(_configuration, _environment);
        var targetFolder = Path.Combine(uploadsRoot, PortalUploadPaths.FeedFolderName);
        Directory.CreateDirectory(targetFolder);

        var safeBaseName = BuildSafeFileName(Path.GetFileNameWithoutExtension(file.FileName));
        var generatedFileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}-{safeBaseName}{extension}";
        var targetPath = Path.Combine(targetFolder, generatedFileName);

        await using (var stream = System.IO.File.Create(targetPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var publicUrl = PortalUploadPaths.BuildFeedPublicUrl(generatedFileName);

        return Ok(new FeedAssetUploadResponse(
            generatedFileName,
            file.ContentType,
            file.Length,
            publicUrl));
    }

    private static string BuildSafeFileName(string fileName)
    {
        var normalized = Regex.Replace(fileName.Trim(), @"[^a-zA-Z0-9-_]+", "-");
        normalized = Regex.Replace(normalized, @"-+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(normalized) ? "foto" : normalized;
    }
}
