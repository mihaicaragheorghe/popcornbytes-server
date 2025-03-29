using System.Diagnostics;

namespace PopcornBytes.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Start processing request {p} {v} {e}{q}",
            context.Request.Protocol,
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString.Value);

        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();
        
        _logger.LogInformation("End processing request {p} {v} {e}{q} in {d}ms",
            context.Request.Protocol,
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString.Value,
            stopwatch.ElapsedMilliseconds);
    }
}