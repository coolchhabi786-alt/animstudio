namespace AnimStudio.ContentModule.Application.Interfaces;

/// <summary>
/// Issues short-lived signed Blob URLs for rendered animation clips.
/// The returned URL expires in 60 seconds (matches the Phase 6 storyboard
/// frame signing pattern).
/// </summary>
public interface IClipUrlSigner
{
    /// <summary>
    /// Returns a signed URL for <paramref name="blobPath"/>, or the input path
    /// unchanged when running in an unconfigured dev environment.
    /// </summary>
    (string Url, DateTimeOffset ExpiresAt) Sign(string blobPath);
}
