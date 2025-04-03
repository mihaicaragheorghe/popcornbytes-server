using Microsoft.AspNetCore.Mvc;

namespace PopcornBytes.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected exception occured: {m}", ex.Message);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal server error",
            };
            
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}