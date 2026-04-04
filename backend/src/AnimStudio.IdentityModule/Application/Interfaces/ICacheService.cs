namespace AnimStudio.IdentityModule.Application.Interfaces;

/// <summary>Redis-backed distributed cache abstraction for the Identity module.</summary>
public interface ICacheService
{
    /// <summary>Returns the cached item or the result of <paramref name="factory"/> if absent, then stores it.</summary>
    Task<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan? absoluteExpiry = null,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a cached item; returns <see langword="null"/> when absent.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>Stores an item in the cache with an optional absolute expiry.</summary>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? absoluteExpiry = null,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a cached item.</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Removes all entries whose keys share the given prefix.</summary>
    Task RemoveByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default);
}
