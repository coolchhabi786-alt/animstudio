using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// Timestamped comment left on a review link.
/// Not a DDD aggregate root — owned by <see cref="ReviewLink"/> — but stored in its
/// own table with a separate repository so comments can be listed independently.
/// </summary>
public sealed class ReviewComment : AggregateRoot<Guid>
{
    public Guid ReviewLinkId { get; private set; }
    public string AuthorName { get; private set; } = string.Empty;
    public string Text { get; private set; } = string.Empty;

    /// <summary>Playback position (seconds) at which the comment was made.</summary>
    public double TimestampSeconds { get; private set; }

    public bool IsResolved { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public Guid? ResolvedByUserId { get; private set; }

    private ReviewComment() { }

    public static ReviewComment Create(
        Guid reviewLinkId,
        string authorName,
        string text,
        double timestampSeconds)
    {
        if (reviewLinkId == Guid.Empty)
            throw new ArgumentException("Review link ID is required.", nameof(reviewLinkId));
        if (string.IsNullOrWhiteSpace(authorName))
            throw new ArgumentException("Author name is required.", nameof(authorName));
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Comment text is required.", nameof(text));

        return new ReviewComment
        {
            Id               = Guid.NewGuid(),
            ReviewLinkId     = reviewLinkId,
            AuthorName       = authorName.Trim(),
            Text             = text.Trim(),
            TimestampSeconds = Math.Max(0, timestampSeconds),
            IsResolved       = false,
            CreatedAt        = DateTimeOffset.UtcNow,
            UpdatedAt        = DateTimeOffset.UtcNow,
        };
    }

    public void Resolve(Guid userId)
    {
        IsResolved       = true;
        ResolvedAt       = DateTime.UtcNow;
        ResolvedByUserId = userId;
        UpdatedAt        = DateTimeOffset.UtcNow;
    }
}
