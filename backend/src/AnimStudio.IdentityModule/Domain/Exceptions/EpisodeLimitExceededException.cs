namespace AnimStudio.IdentityModule.Domain.Exceptions;

/// <summary>Thrown when creating an episode would exceed the plan's EpisodesPerMonth limit.</summary>
public sealed class EpisodeLimitExceededException : DomainException
{
    public int Limit { get; }
    public int Used { get; }

    public EpisodeLimitExceededException(int used, int limit)
        : base($"Monthly episode limit reached ({used}/{limit}). Upgrade your plan to create more episodes.",
               "EPISODE_LIMIT_EXCEEDED")
    {
        Used = used;
        Limit = limit;
    }
}
