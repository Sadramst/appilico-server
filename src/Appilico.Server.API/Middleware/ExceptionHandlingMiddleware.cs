using System.Net;
using System.Text.Json;
using Appilico.Server.Business.Exceptions;
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
    private readonly IHostEnvironment _environment;

    /// <summary>Initializes ExceptionHandlingMiddleware.</summary>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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
            NotSupportedException => HttpStatusCode.ServiceUnavailable,
            StorageProviderException => HttpStatusCode.ServiceUnavailable,
            PaymentProviderException => HttpStatusCode.ServiceUnavailable,
            InvalidOperationException => HttpStatusCode.Conflict,
            _ => HttpStatusCode.InternalServerError
        };

        _logger.LogError(exception, "Unhandled exception. TraceId={TraceId}", context.TraceIdentifier);

        var message = ShouldExposeMessage(exception, statusCode)
            ? exception.Message
            : "The request could not be completed.";

        var response = ApiResponse<object>.FailResponse(message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        context.Response.Headers["X-Correlation-Id"] = context.TraceIdentifier;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private bool ShouldExposeMessage(Exception exception, HttpStatusCode statusCode)
    {
        if (_environment.IsDevelopment())
            return true;

        return exception is ValidationException
            || exception is ArgumentException
            || exception is UnauthorizedAccessException
            || exception is KeyNotFoundException
            || statusCode == HttpStatusCode.ServiceUnavailable;
    }
}
