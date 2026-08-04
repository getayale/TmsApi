using System.Diagnostics;

namespace TmsApi.Api.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate correlation ID
        var correlationId = Guid.NewGuid()
            .ToString("N")[..8];

        // Add correlation ID to response header before pipeline continues
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        // Start measuring request time
        var stopwatch = Stopwatch.StartNew();

        // Log incoming request
        _logger.LogInformation(
            "Request started: {Method} {Path} CorrelationId={CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        // Pass request to next middleware / endpoint
        await _next(context);

        // Stop timer after response is generated
        stopwatch.Stop();

        // Log completed request
        _logger.LogInformation(
            "Request completed: StatusCode={StatusCode} ElapsedMs={ElapsedMs} CorrelationId={CorrelationId}",
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            correlationId);
    }
}