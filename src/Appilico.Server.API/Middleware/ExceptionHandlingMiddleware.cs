using System.Net;
using System.Text.Json;
using Appilico.Server.Business.DTOs.Common;
using FluentValidation;

namespace Appilico.Server.API.Middleware;

/// <summary>
/// Global exception handling middleware.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>Initializes ExceptionHandlingMiddleware.</summary>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Invoke.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            ValidationException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            ArgumentException => HttpStatusCode.BadRequest,
            InvalidOperationException => HttpStatusCode.Conflict,
            _ => HttpStatusCode.InternalServerError
        };

        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var message = statusCode == HttpStatusCode.InternalServerError
            ? "An internal server error occurred."
            : exception.Message;

        var response = ApiResponse<object>.FailResponse(message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
