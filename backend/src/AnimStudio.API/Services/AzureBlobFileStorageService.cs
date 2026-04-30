using AnimStudio.ContentModule.Application.Interfaces;

namespace AnimStudio.API.Services;

/// <summary>
/// Production file storage backed by Azure Blob Storage.
/// Uses the existing <see cref="IClipUrlSigner"/> to generate short-lived SAS URLs.
/// Activated when <c>FileStorage:Provider = "AzureBlob"</c> in configuration.
/// <para>
/// <b>Phase 9 note:</b> <see cref="SaveFileAsync"/> is not yet implemented —
/// uploads will be added when the render/voice pipeline uploads results to Blob.
/// </para>
/// </summary>
public sealed class AzureBlobFileStorageService(IClipUrlSigner signer) : IFileStorageService
{
    public Task<string> SaveFileAsync(
        Stream content,
        string relativePath,
        string contentType,
        CancellationToken ct = default)
        => throw new NotSupportedException(
            "Azure Blob upload is not yet implemented. Set FileStorage:Provider=Local for dev.");

    public string GetFileUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return relativePath;

        // Already an absolute URL (e.g. already-signed SAS URL stored in DB) — return as-is
        if (relativePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            relativePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return relativePath;

        var (url, _) = signer.Sign(relativePath);
        return url;
    }

    public Task<bool> ExistsAsync(string relativePath, CancellationToken ct = default)
        => Task.FromResult(true); // Assume blob exists; real check would call BlobClient.ExistsAsync()
}
