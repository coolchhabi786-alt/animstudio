using AnimStudio.ContentModule.Application.Interfaces;

namespace AnimStudio.API.Services;

/// <summary>
/// Stores files on the local filesystem and serves them via the
/// <c>FileStorageController</c> at <c>/api/v1/files/{relativePath}</c>.
/// Activated when <c>FileStorage:Provider = "Local"</c> in configuration.
/// </summary>
public sealed class LocalFileStorageService(
    IConfiguration configuration,
    ILogger<LocalFileStorageService> logger) : IFileStorageService
{
    private readonly string _rootPath = configuration["FileStorage:LocalRootPath"]
        ?? throw new InvalidOperationException("FileStorage:LocalRootPath is required when Provider=Local.");

    private readonly string _backendBaseUrl = (configuration["FileStorage:BackendBaseUrl"]
        ?? "http://localhost:5001").TrimEnd('/');

    public async Task<string> SaveFileAsync(
        Stream content,
        string relativePath,
        string contentType,
        CancellationToken ct = default)
    {
        var fullPath = ToFullPath(relativePath);
        var dir = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(dir);

        await using var file = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(file, ct);

        logger.LogDebug("Saved file to local storage: {RelativePath}", relativePath);
        return relativePath;
    }

    public string GetFileUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return relativePath;

        // Already an absolute URL — return as-is (handles pre-existing blob paths in DB)
        if (relativePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            relativePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return relativePath;

        var urlPath = relativePath.Replace('\\', '/').TrimStart('/');
        return $"{_backendBaseUrl}/api/v1/files/{urlPath}";
    }

    public Task<bool> ExistsAsync(string relativePath, CancellationToken ct = default)
        => Task.FromResult(File.Exists(ToFullPath(relativePath)));

    private string ToFullPath(string relativePath)
        => Path.GetFullPath(Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar)));
}
