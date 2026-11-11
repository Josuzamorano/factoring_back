using System.Net;
using System.Text.Json;

namespace Factoring.API.Middleware;

/// <summary>
/// Middleware para manejo global de excepciones
/// Captura todas las excepciones no manejadas y retorna respuestas consistentes
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

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
        var correlationId = context.TraceIdentifier;
        var path = context.Request.Path;
        var method = context.Request.Method;
        var user = context.User?.Identity?.Name ?? "anonymous";

        // Log estructurado con contexto
        _logger.LogError(exception,
            "Error no manejado. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}, User: {User}",
            correlationId, path, method, user);

        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            // Errores de validacion
            ArgumentException argEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Mensaje = argEx.Message,
                Tipo = "ValidationError",
                CorrelationId = correlationId
            },

            // Errores de autenticacion
            UnauthorizedAccessException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Mensaje = "No tiene permisos para realizar esta accion",
                Tipo = "AuthorizationError",
                CorrelationId = correlationId
            },

            // Recursos no encontrados
            KeyNotFoundException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Mensaje = "El recurso solicitado no fue encontrado",
                Tipo = "NotFoundError",
                CorrelationId = correlationId
            },

            // Errores de negocio personalizados
            BusinessException bizEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Mensaje = bizEx.Message,
                Tipo = "BusinessError",
                Codigo = bizEx.ErrorCode,
                CorrelationId = correlationId
            },

            // Timeout de base de datos
            TimeoutException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.GatewayTimeout,
                Mensaje = "La operacion tomo demasiado tiempo. Por favor intente nuevamente.",
                Tipo = "TimeoutError",
                CorrelationId = correlationId
            },

            // Error generico
            _ => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Mensaje = _env.IsDevelopment()
                    ? exception.Message
                    : "Ha ocurrido un error interno. Por favor contacte al administrador.",
                Tipo = "InternalError",
                CorrelationId = correlationId,
                Detalle = _env.IsDevelopment() ? exception.StackTrace : null
            }
        };

        context.Response.StatusCode = response.StatusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}

/// <summary>
/// Modelo de respuesta de error estandarizado
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Excepcion personalizada para errores de negocio
/// </summary>
public class BusinessException : Exception
{
    public string ErrorCode { get; }

    public BusinessException(string message, string errorCode = "BIZ_ERROR")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public BusinessException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Extension para registrar el middleware
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
