using System.Text.Json;
using ECommerce.Application.Exceptions;

namespace ECommerce.API.Middleware;

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
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (status, message) = ex switch
        {
            NotFoundException e      => (404, e.Message),
            BadRequestException e    => (400, e.Message),
            UnauthorizedException e  => (401, e.Message),
            ForbiddenException e     => (403, e.Message),
            ConflictException e      => (409, e.Message),
            _                        => (500, "An unexpected error occurred.")
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new
        {
            statusCode = status,
            message,
            errors = Array.Empty<string>()
        });

        return context.Response.WriteAsync(body);
    }
}
