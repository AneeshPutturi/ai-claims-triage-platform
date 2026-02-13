// =============================================
// Middleware: ErrorHandlingMiddleware
// Description: Global error handling with consistent format
// Author: API Team
// Date: February 2026
// =============================================

using System.Net;
using System.Text.Json;
using ClaimsIntake.API.Controllers;

namespace ClaimsIntake.API.Middleware;

/// <summary>
/// Middleware that catches exceptions and returns consistent error responses.
/// No stack traces leaked to clients.
/// </summary>
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
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error occurred");
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, "Validation error");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation");
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, "Invalid operation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        HttpStatusCode statusCode,
        string error)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var correlationId = context.Items["CorrelationId"]?.ToString();

        var response = new ErrorResponse(
            Error: error,
            Message: exception.Message,
            CorrelationId: correlationId);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
