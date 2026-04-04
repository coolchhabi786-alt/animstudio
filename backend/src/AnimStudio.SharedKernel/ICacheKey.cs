namespace AnimStudio.SharedKernel
{
    /// <summary>
    /// Marks a query as cacheable. Implement on MediatR request records.
    /// The <see cref="CachingBehaviour{TRequest,TResponse}"/> reads both
    /// <see cref="Key"/> and <see cref="CacheDuration"/> automatically.
    /// </summary>
    public interface ICacheKey
    {
        /// <summary>Unique Redis key for this query result.</summary>
        string Key { get; }

        /// <summary>How long the result should be cached (absolute expiry from now).</summary>
        TimeSpan CacheDuration { get; }
    }
}