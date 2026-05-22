using Serilog.Context;

namespace Appilico.Server.API.Middleware;

/// <summary>Adds a stable correlation id to each request and response.</summary>
public sealed class CorrelationIdMiddleware
{
    /// <summary>Correlation id header name.</summary>
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    /// <summary>Initialises the middleware.</summary>
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>Runs the middleware.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);
        context.TraceIdentifier = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var values)
            && !string.IsNullOrWhiteSpace(values.FirstOrDefault()))
        {
            return values.First()!;
        }

        return Guid.NewGuid().ToString("N");
    }
}