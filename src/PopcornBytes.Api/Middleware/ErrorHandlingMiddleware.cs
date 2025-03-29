using System.Net;

using Microsoft.AspNetCore.Mvc;

using PopcornBytes.Api.Kernel;

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
        catch (ServiceException ex)
        {
            _logger.LogError(ex, "{m}", ex.Message);

            var problemDetails = new ProblemDetails
            {
                Status = ex.Error.StatusCode, Title = ex.Error.Code, Detail = ex.Error.Message,
            };

            context.Response.StatusCode = ex.Error.StatusCode;
            await context.Response.WriteAsJsonAsync(problemDetails);
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