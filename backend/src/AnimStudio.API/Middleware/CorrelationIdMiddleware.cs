namespace AnimStudio.API.Middleware;

/// <summary>
/// Ensures every request carries a correlation ID (from header or newly generated).
/// Forwards the correlation ID in the response.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                            ?? Guid.NewGuid().ToString("N");

        context.TraceIdentifier = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        await next(context);
    }
}
