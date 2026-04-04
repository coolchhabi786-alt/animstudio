using AnimStudio.SharedKernel.Behaviours;

namespace AnimStudio.API.Services;

/// <summary>
/// Reads the correlation ID from <see cref="IHttpContextAccessor.HttpContext.TraceIdentifier"/>,
/// which is set by <see cref="AnimStudio.API.Middleware.CorrelationIdMiddleware"/> from the
/// incoming X-Correlation-ID header (or a freshly generated UUID).
/// </summary>
internal sealed class HttpContextCorrelationIdProvider(IHttpContextAccessor httpContextAccessor)
    : ICorrelationIdProvider
{
    public string GetCorrelationId() =>
        httpContextAccessor.HttpContext?.TraceIdentifier ?? string.Empty;
}
