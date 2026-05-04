using AnimStudio.ContentModule.Application.Interfaces;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace AnimStudio.API.Services;

/// <summary>
/// Production file storage backed by Azure Blob Storage.
/// Uses DefaultAzureCredential — works with Managed Identity in prod and <c>az login</c> in dev.
/// Activated when <c>FileStorage:Provider = "AzureBlob"</c> in configuration.
/// </summary>
public sealed class AzureBlobFileStorageService : IFileStorageService
{
    private const string AssetsContainer = "assets";
    private const string PreviewsContainer = "previews";

    private readonly BlobServiceClient _serviceClient;
    private readonly string _accountName;
    private readonly string _cdnEndpoint;

    // UserDelegationKey is valid for up to 7 days; we refresh 5 minutes before expiry.
    private Azure.Storage.Blobs.Models.UserDelegationKey? _delegationKey;
    private DateTimeOffset _delegationKeyExpiry = DateTimeOffset.MinValue;
    private readonly object _keyLock = new();

    public AzureBlobFileStorageService(IConfiguration configuration)
    {
        _accountName = configuration["BlobStorage:AccountName"]
            ?? throw new InvalidOperationException("BlobStorage:AccountName is required when FileStorage:Provider=AzureBlob.");

        _cdnEndpoint = (configuration["BlobStorage:CdnEndpoint"]
            ?? throw new InvalidOperationException("BlobStorage:CdnEndpoint is required when FileStorage:Provider=AzureBlob."))
            .TrimEnd('/');

        _serviceClient = new BlobServiceClient(
            new Uri($"https://{_accountName}.blob.core.windows.net"),
            new DefaultAzureCredential());
    }

    public async Task<string> SaveFileAsync(
        Stream content,
        string blobPath,
        string contentType,
        CancellationToken ct = default)
    {
        var blob = _serviceClient
            .GetBlobContainerClient(AssetsContainer)
            .GetBlobClient(blobPath.TrimStart('/'));

        await blob.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType,
                CacheControl = "public, max-age=31536000",  // 1 year — assets are immutable by path
            }
        }, ct);

        return GetCdnUrl(blobPath);
    }

    public string GetFileUrl(string relativePath, TimeSpan? expiry = null)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return relativePath;

        if (relativePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            relativePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return relativePath;

        return expiry is null
            ? GetCdnUrl(relativePath)
            : BuildSasUrl(AssetsContainer, relativePath, expiry.Value);
    }

    public async Task<string> SavePreviewAsync(
        Stream content,
        string previewPath,
        string contentType,
        CancellationToken ct = default)
    {
        var blob = _serviceClient
            .GetBlobContainerClient(PreviewsContainer)
            .GetBlobClient(previewPath.TrimStart('/'));

        await blob.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType,
                CacheControl = "no-cache, no-store",
            }
        }, ct);

        return BuildSasUrl(PreviewsContainer, previewPath, TimeSpan.FromHours(1));
    }

    public async Task<bool> ExistsAsync(string relativePath, CancellationToken ct = default)
    {
        var blob = _serviceClient
            .GetBlobContainerClient(AssetsContainer)
            .GetBlobClient(relativePath.TrimStart('/'));

        var response = await blob.ExistsAsync(ct);
        return response.Value;
    }

    public async Task DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        var blob = _serviceClient
            .GetBlobContainerClient(AssetsContainer)
            .GetBlobClient(relativePath.TrimStart('/'));

        // Soft delete: stamp metadata so Azure lifecycle policy hard-deletes after 7 days.
        await blob.SetMetadataAsync(
            new Dictionary<string, string> { ["DeletedAt"] = DateTimeOffset.UtcNow.ToString("O") },
            cancellationToken: ct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string GetCdnUrl(string blobPath)
        => $"{_cdnEndpoint}/{blobPath.TrimStart('/')}";

    private string BuildSasUrl(string containerName, string blobPath, TimeSpan expiry)
    {
        var delegationKey = GetOrRefreshDelegationKey();
        var expiresOn = DateTimeOffset.UtcNow.Add(expiry);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobPath.TrimStart('/'),
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),  // absorb clock skew
            ExpiresOn = expiresOn,
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasParams = sasBuilder.ToSasQueryParameters(delegationKey, _accountName);
        return $"https://{_accountName}.blob.core.windows.net/{containerName}/{blobPath.TrimStart('/')}?{sasParams}";
    }

    // Double-checked lock: GetUserDelegationKey is a synchronous REST call we cache for 12 hours.
    private Azure.Storage.Blobs.Models.UserDelegationKey GetOrRefreshDelegationKey()
    {
        if (_delegationKey is not null && DateTimeOffset.UtcNow < _delegationKeyExpiry.AddMinutes(-5))
            return _delegationKey;

        lock (_keyLock)
        {
            if (_delegationKey is not null && DateTimeOffset.UtcNow < _delegationKeyExpiry.AddMinutes(-5))
                return _delegationKey;

            var keyExpiry = DateTimeOffset.UtcNow.AddHours(12);
            _delegationKey = _serviceClient
                .GetUserDelegationKey(DateTimeOffset.UtcNow, keyExpiry)
                .Value;
            _delegationKeyExpiry = keyExpiry;
            return _delegationKey;
        }
    }
}
