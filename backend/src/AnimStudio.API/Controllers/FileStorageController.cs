using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnimStudio.API.Controllers;

/// <summary>
/// Serves files from the local filesystem when <c>FileStorage:Provider = "Local"</c>.
/// In production (AzureBlob provider) this controller returns 404 for all requests —
/// clients download directly from Azure Blob via SAS URLs instead.
///
/// Route: <c>GET /api/v1/files/{**filePath}</c>
/// No authentication required — URLs must be kept short-lived or non-guessable in production.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/v1/files")]
public sealed class FileStorageController(IConfiguration configuration) : ControllerBase
{
    private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".mp4"]  = "video/mp4",
        [".webm"] = "video/webm",
        [".mp3"]  = "audio/mpeg",
        [".wav"]  = "audio/wav",
        [".png"]  = "image/png",
        [".jpg"]  = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"]  = "image/gif",
        [".webp"] = "image/webp",
        [".svg"]  = "image/svg+xml",
    };

    [HttpGet("{**filePath}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult ServeFile(string filePath)
    {
        var provider = configuration["FileStorage:Provider"] ?? "AzureBlob";
        if (!string.Equals(provider, "Local", StringComparison.OrdinalIgnoreCase))
            return NotFound();

        var rootPath = configuration["FileStorage:LocalRootPath"];
        if (string.IsNullOrWhiteSpace(rootPath))
            return NotFound();

        var resolvedRoot = Path.GetFullPath(rootPath);
        var resolvedFile = Path.GetFullPath(Path.Combine(resolvedRoot, filePath));

        // Path traversal guard
        if (!resolvedFile.StartsWith(resolvedRoot, StringComparison.OrdinalIgnoreCase))
            return Forbid();

        if (!System.IO.File.Exists(resolvedFile))
            return NotFound();

        var ext = Path.GetExtension(resolvedFile);
        var contentType = ContentTypes.GetValueOrDefault(ext, "application/octet-stream");

        // PhysicalFile with enableRangeProcessing handles HTTP range requests automatically
        // (needed for browser video seeking)
        Response.Headers.CacheControl = "public, max-age=3600";
        return PhysicalFile(resolvedFile, contentType, enableRangeProcessing: true);
    }
}
