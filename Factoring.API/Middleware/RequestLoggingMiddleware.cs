using System.Diagnostics;

namespace Factoring.API.Middleware;

/// <summary>
/// Middleware para logging de requests HTTP
/// Registra informacion de cada request: metodo, path, tiempo de respuesta, usuario
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.TraceIdentifier;
        var method = context.Request.Method;
        var path = context.Request.Path;
        var queryString = context.Request.QueryString;
        var user = context.User?.Identity?.Name ?? "anonymous";
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Agregar correlation ID al response header
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Log de inicio (solo para debugging, nivel Debug)
            _logger.LogDebug(
                "Request iniciado. CorrelationId: {CorrelationId}, Method: {Method}, Path: {Path}{Query}, User: {User}, IP: {IP}",
                correlationId, method, path, queryString, user, ip);

            await _next(context);

            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Log de finalizacion con nivel segun resultado
            if (statusCode >= 500)
            {
                _logger.LogError(
                    "Request completado con error. CorrelationId: {CorrelationId}, Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, ElapsedMs: {ElapsedMs}, User: {User}",
                    correlationId, method, path, statusCode, elapsedMs, user);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning(
                    "Request completado con advertencia. CorrelationId: {CorrelationId}, Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, ElapsedMs: {ElapsedMs}, User: {User}",
                    correlationId, method, path, statusCode, elapsedMs, user);
            }
            else
            {
                _logger.LogInformation(
                    "Request completado. CorrelationId: {CorrelationId}, Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, ElapsedMs: {ElapsedMs}, User: {User}",
                    correlationId, method, path, statusCode, elapsedMs, user);
            }

            // Log de advertencia si el request fue muy lento (> 3 segundos)
            if (elapsedMs > 3000)
            {
                _logger.LogWarning(
                    "Request lento detectado. CorrelationId: {CorrelationId}, Method: {Method}, Path: {Path}, ElapsedMs: {ElapsedMs}",
                    correlationId, method, path, elapsedMs);
            }
        }
        catch (Exception)
        {
            stopwatch.Stop();
            // El error sera manejado por GlobalExceptionMiddleware
            throw;
        }
    }
}

/// <summary>
/// Extension para registrar el middleware
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
