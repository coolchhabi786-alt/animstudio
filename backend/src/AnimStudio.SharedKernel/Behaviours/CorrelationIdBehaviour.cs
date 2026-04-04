using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace AnimStudio.SharedKernel.Behaviours
{
    /// <summary>
    /// Pipeline behavior for managing a correlation ID.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class CorrelationIdBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ICorrelationIdProvider _correlationIdProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationIdBehaviour{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="correlationIdProvider">The provider for correlation ID.</param>
        public CorrelationIdBehaviour(ICorrelationIdProvider correlationIdProvider)
        {
            _correlationIdProvider = correlationIdProvider;
        }

        /// <inheritdoc />
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var correlationId = _correlationIdProvider.GetCorrelationId();
            if (!string.IsNullOrEmpty(correlationId))
            {
                // Add correlation ID to request or logging context here if needed
            }

            return await next();
        }
    }

    /// <summary>
    /// Interface for obtaining the correlation ID.
    /// </summary>
    public interface ICorrelationIdProvider
    {
        /// <summary>
        /// Gets the correlation ID.
        /// </summary>
        /// <returns>The correlation ID as a string.</returns>
        string GetCorrelationId();
    }
}