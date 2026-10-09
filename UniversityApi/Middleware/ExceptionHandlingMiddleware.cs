using UniversityApi.Common;

namespace UniversityApi.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);

            var result = ReturnResult<object>.Fail(StatusCodes.Status500InternalServerError, "INTERNAL_SERVER_ERROR", "An unexpected error occurred");
            result.TraceId = context.TraceIdentifier;

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(result);
        }
    }
}
