// =============================================
// Middleware: CorrelationIdMiddleware
// Description: Injects correlation ID into every request
// Author: API Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.API.Middleware;

/// <summary>
/// Middleware that injects a correlation ID into every request.
/// ID is returned in response headers and propagated to logs.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate or extract correlation ID
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        // Store in context for access by other middleware/controllers
        context.Items["CorrelationId"] = correlationId;

        // Add to response headers
        context.Response.Headers.Add(CorrelationIdHeader, correlationId);

        // Add to logging scope
        using (context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger<CorrelationIdMiddleware>()
            .BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await _next(context);
        }
    }
}
