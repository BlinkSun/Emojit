using System.Diagnostics;

namespace EmojitServer.Api.Middleware;

/// <summary>
/// Provides cross-cutting structured logging for inbound HTTP requests handled by the API.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware component in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Logs the lifecycle of the current HTTP request.
    /// </summary>
    /// <param name="context">The HTTP context representing the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string method = context.Request.Method;
        PathString path = context.Request.Path;
        string traceIdentifier = context.TraceIdentifier;
        Stopwatch stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Handling {Method} {Path} ({TraceIdentifier}).",
            method,
            path,
            traceIdentifier);

        try
        {
            await _next(context).ConfigureAwait(false);

            stopwatch.Stop();
            _logger.LogInformation(
                "Handled {Method} {Path} with status {StatusCode} in {ElapsedMilliseconds} ms ({TraceIdentifier}).",
                method,
                path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                traceIdentifier);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Unhandled exception while processing {Method} {Path} after {ElapsedMilliseconds} ms ({TraceIdentifier}).",
                method,
                path,
                stopwatch.ElapsedMilliseconds,
                traceIdentifier);
            throw;
        }
    }
}
