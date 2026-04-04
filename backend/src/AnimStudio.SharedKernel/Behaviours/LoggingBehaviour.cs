using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Serilog;

namespace AnimStudio.SharedKernel.Behaviours
{
    /// <summary>
    /// Pipeline behavior for logging request and response details.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingBehaviour{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="logger">The Serilog logger instance.</param>
        public LoggingBehaviour(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            _logger.Information("Handling {RequestName}: {@Request}", requestName, request);

            var stopwatch = Stopwatch.StartNew();
            var response = await next();
            stopwatch.Stop();

            var responseName = typeof(TResponse).Name;
            _logger.Information("Handled {RequestName} in {ElapsedMilliseconds}ms: {@Response}", requestName, stopwatch.ElapsedMilliseconds, response);

            return response;
        }
    }
}