namespace AnimStudio.ContentModule.Application.Interfaces;

/// <summary>
/// Abstracts file storage so the same application code works with a local
/// filesystem (dev) or Azure Blob Storage (production).
/// Toggle via <c>FileStorage:Provider</c> in appsettings ("Local" | "AzureBlob").
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Persists <paramref name="content"/> at <paramref name="relativePath"/> within the
    /// configured storage root ("assets" container for blob). Returns the stored path / CDN URL.
    /// </summary>
    Task<string> SaveFileAsync(
        Stream content,
        string relativePath,
        string contentType,
        CancellationToken ct = default);

    /// <summary>
    /// Converts a stored path into a fully-qualified URL the browser can fetch.
    /// <para>
    /// Local: <c>http://localhost:5001/api/v1/files/{relativePath}</c> (expiry ignored).
    /// AzureBlob with <paramref name="expiry"/> = <c>null</c>: CDN URL (public, no expiry).
    /// AzureBlob with <paramref name="expiry"/> set: UserDelegationKey SAS URL.
    /// </para>
    /// Returns <paramref name="relativePath"/> unchanged when it is already an absolute URL.
    /// </summary>
    string GetFileUrl(string relativePath, TimeSpan? expiry = null);

    /// <summary>
    /// Uploads <paramref name="content"/> to the previews container and returns a 1-hour SAS URL.
    /// For local storage: saves to the same root and returns the local file URL.
    /// </summary>
    Task<string> SavePreviewAsync(
        Stream content,
        string previewPath,
        string contentType,
        CancellationToken ct = default);

    /// <summary>Returns <c>true</c> when the file at <paramref name="relativePath"/> exists.</summary>
    Task<bool> ExistsAsync(string relativePath, CancellationToken ct = default);

    /// <summary>
    /// Soft-deletes the file at <paramref name="relativePath"/>.
    /// AzureBlob: sets <c>DeletedAt</c> metadata; lifecycle policy hard-deletes after 7 days.
    /// Local: deletes the file from disk immediately.
    /// </summary>
    Task DeleteAsync(string relativePath, CancellationToken ct = default);
}
