using AnimStudio.ContentModule.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AnimStudio.API.Services;

/// <summary>
/// Stamps a short-lived signed URL onto a blob path. Production implementation
/// should call <c>BlobClient.GenerateSasUri</c>; the current implementation
/// appends query-string metadata so the URL round-trips for dev + e2e tests
/// without bringing Azure.Storage.Blobs into this module's hot path.
/// </summary>
public sealed class BlobClipUrlSigner(IConfiguration configuration) : IClipUrlSigner
{
    public (string Url, DateTimeOffset ExpiresAt) Sign(string blobPath)
    {
        var expires = DateTimeOffset.UtcNow.AddSeconds(60);
        var baseUrl = configuration["AzureBlob:ClipsCdnBase"];

        // If a CDN/Blob base URL is configured, prepend it for relative paths.
        var absolute = !string.IsNullOrWhiteSpace(baseUrl)
                       && !blobPath.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? $"{baseUrl.TrimEnd('/')}/{blobPath.TrimStart('/')}"
            : blobPath;

        var separator = absolute.Contains('?') ? '&' : '?';
        var signed = $"{absolute}{separator}se={Uri.EscapeDataString(expires.ToString("O"))}";
        return (signed, expires);
    }
}
