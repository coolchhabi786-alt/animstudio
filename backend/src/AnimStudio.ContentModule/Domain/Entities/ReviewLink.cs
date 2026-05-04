using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// Shareable review link for a completed render.
/// Token is a 16-char hex string derived from a fresh GUID.
/// Supports optional expiry and optional BCrypt-hashed password.
/// </summary>
public sealed class ReviewLink : AggregateRoot<Guid>
{
    /// <summary>16-character hex token used in the public share URL.</summary>
    public string Token { get; private set; } = string.Empty;
    public Guid EpisodeId { get; private set; }
    public Guid RenderId { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public string? PasswordHash { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public int ViewCount { get; private set; }

    private ReviewLink() { }

    public static ReviewLink Create(
        Guid episodeId,
        Guid renderId,
        Guid createdByUserId,
        DateTime? expiresAt = null,
        string? passwordHash = null)
    {
        if (episodeId == Guid.Empty)
            throw new ArgumentException("Episode ID is required.", nameof(episodeId));
        if (renderId == Guid.Empty)
            throw new ArgumentException("Render ID is required.", nameof(renderId));

        return new ReviewLink
        {
            Id              = Guid.NewGuid(),
            Token           = Guid.NewGuid().ToString("N")[..16],
            EpisodeId       = episodeId,
            RenderId        = renderId,
            CreatedByUserId = createdByUserId,
            ExpiresAt       = expiresAt,
            PasswordHash    = passwordHash,
            IsRevoked       = false,
            ViewCount       = 0,
            CreatedAt       = DateTimeOffset.UtcNow,
            UpdatedAt       = DateTimeOffset.UtcNow,
        };
    }

    public void Revoke()
    {
        IsRevoked = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Returns true when the link is neither revoked nor past its expiry.</summary>
    public bool IsValid()
        => !IsRevoked && (ExpiresAt is null || ExpiresAt.Value > DateTime.UtcNow);
}
