using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using PortalRH.Api.Contracts.Admin.Polls;
using PortalRH.Api.Security;

namespace PortalRH.Api.Controllers;

[ApiController]
[Route("api/admin/polls/assets")]
[RequirePortalSession]
public class AdminPollAssetsController : ControllerBase
{
    private static readonly HashSet<string> AllowedImageExtensions =
    [
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    ];

    private static readonly HashSet<string> AllowedAttachmentExtensions =
    [
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".zip"
    ];

    private readonly IWebHostEnvironment _environment;

    public AdminPollAssetsController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost("{assetType}")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    [ProducesResponseType(typeof(PollAssetUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Upload(string assetType, IFormFile? file, CancellationToken cancellationToken)
    {
        var session = PortalSessionHttpContext.Get(HttpContext);
        if (session?.PortalUser is null)
        {
            return Unauthorized(new { message = "Sessao do portal nao encontrada." });
        }

        if (!PortalModuleAccess.CanManagePolls(session.PortalUser))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Voce nao possui permissao para gerenciar enquetes."
            });
        }

        if (file is null || file.Length <= 0)
        {
            return BadRequest(new { message = "Selecione um arquivo para upload." });
        }

        var normalizedType = NormalizeAssetType(assetType);
        if (normalizedType is null)
        {
            return BadRequest(new { message = "Tipo de ativo invalido para enquete." });
        }

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
        var allowed = normalizedType == "image" ? AllowedImageExtensions : AllowedAttachmentExtensions;
        if (!allowed.Contains(extension))
        {
            return BadRequest(new
            {
                message = normalizedType == "image"
                    ? "Formato de imagem nao suportado."
                    : "Formato de anexo nao suportado."
            });
        }

        var rootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            rootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var targetFolder = Path.Combine(rootPath, "uploads", "polls", normalizedType);
        Directory.CreateDirectory(targetFolder);

        var safeBaseName = BuildSafeFileName(Path.GetFileNameWithoutExtension(file.FileName));
        var generatedFileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}-{safeBaseName}{extension}";
        var targetPath = Path.Combine(targetFolder, generatedFileName);

        await using (var stream = System.IO.File.Create(targetPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var publicUrl = $"{Request.Scheme}://{Request.Host}/uploads/polls/{normalizedType}/{generatedFileName}";

        return Ok(new PollAssetUploadResponse(
            normalizedType,
            generatedFileName,
            file.ContentType,
            file.Length,
            publicUrl));
    }

    private static string? NormalizeAssetType(string? assetType)
    {
        var normalized = assetType?.Trim().ToLowerInvariant();
        return normalized is "image" or "attachment" ? normalized : null;
    }

    private static string BuildSafeFileName(string fileName)
    {
        var normalized = Regex.Replace(fileName.Trim(), @"[^a-zA-Z0-9-_]+", "-");
        normalized = Regex.Replace(normalized, @"-+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(normalized) ? "arquivo" : normalized;
    }
}
