using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using System.Text.Json;

namespace AnimStudio.SharedKernel.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that caches query responses in Redis.
/// Only activates when <typeparamref name="TRequest"/> implements <see cref="ICacheKey"/>.
/// Uses the Polly "redis" resilience pipeline so a Redis outage degrades gracefully
/// to a cache miss — the query still executes against the database.
/// </summary>
public sealed class CachingBehaviour<TRequest, TResponse>(
    IDistributedCache cache,
    ResiliencePipelineProvider<string> resilience,
    ILogger<CachingBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only cache requests that declare a cache key + duration
        if (request is not ICacheKey cacheKey)
            return await next();

        var pipeline = resilience.GetPipeline("redis");

        // Try Redis GET — failures are absorbed by the Polly fallback
        TResponse? cached = default;
        try
        {
            await pipeline.ExecuteAsync(async ct =>
            {
                var bytes = await cache.GetAsync(cacheKey.Key, ct);
                if (bytes is { Length: > 0 })
                {
                    cached = JsonSerializer.Deserialize<TResponse>(bytes, JsonOpts);
                    logger.LogDebug("Cache HIT for key {CacheKey}", cacheKey.Key);
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache GET failed for key {CacheKey}; falling through to DB", cacheKey.Key);
        }

        if (cached is not null)
            return cached;

        // Cache miss — execute the actual handler
        logger.LogDebug("Cache MISS for key {CacheKey}", cacheKey.Key);
        var response = await next();

        // Try Redis SET — only cache successful results.
        // Result<T> implements IResult; a Failure result must never be cached because
        // it would suppress the real value for the entire TTL (e.g. a subscription that
        // is created moments after the first 404 would continue returning 404 from cache).
        if (response is not null && response is not IResult { IsSuccess: false })
        {
            try
            {
                await pipeline.ExecuteAsync(async ct =>
                {
                    var bytes = JsonSerializer.SerializeToUtf8Bytes(response, JsonOpts);
                    await cache.SetAsync(cacheKey.Key, bytes,
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = cacheKey.CacheDuration,
                        }, ct);
                    logger.LogDebug("Cache SET key {CacheKey} TTL {TTL}", cacheKey.Key, cacheKey.CacheDuration);
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Cache SET failed for key {CacheKey}; ignoring", cacheKey.Key);
            }
        }

        return response;
    }
}