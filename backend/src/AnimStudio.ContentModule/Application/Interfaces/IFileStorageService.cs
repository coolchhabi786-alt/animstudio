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
    /// configured storage root. Returns the relative path so callers can store it in the DB.
    /// </summary>
    Task<string> SaveFileAsync(
        Stream content,
        string relativePath,
        string contentType,
        CancellationToken ct = default);

    /// <summary>
    /// Converts a stored relative path into a fully-qualified URL the browser can fetch.
    /// For local storage: <c>http://localhost:5001/api/v1/files/{relativePath}</c>.
    /// For Azure Blob: a short-lived SAS URL.
    /// Returns <paramref name="relativePath"/> unchanged when it is already an absolute URL.
    /// </summary>
    string GetFileUrl(string relativePath);

    /// <summary>Returns <c>true</c> when the file at <paramref name="relativePath"/> exists.</summary>
    Task<bool> ExistsAsync(string relativePath, CancellationToken ct = default);
}
