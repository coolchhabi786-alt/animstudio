using System;
using System.Threading;
using System.Threading.Tasks;

namespace AnimStudio.IdentityModule.Application.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T?>> factory, TimeSpan? absoluteExpiry = null, CancellationToken cancellationToken = default);
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiry = null, CancellationToken cancellationToken = default);
        Task InvalidateAsync(string key, CancellationToken cancellationToken = default);
        Task InvalidateByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    }
}
