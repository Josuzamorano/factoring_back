using System.Collections.Concurrent;
using System.Net;

namespace Factoring.API.Middleware;

/// <summary>
/// Middleware para limitar la tasa de requests (Rate Limiting)
/// Protege contra ataques DoS y abuso de API
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitingOptions _options;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    // Cache de requests por cliente
    private static readonly ConcurrentDictionary<string, ClientRequestInfo> _clientRequests = new();

    // Limpieza periodica
    private static DateTime _lastCleanup = DateTime.UtcNow;
    private static readonly object _cleanupLock = new();

    public RateLimitingMiddleware(
        RequestDelegate next,
        RateLimitingOptions options,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _options = options;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Limpiar registros antiguos periodicamente
        CleanupOldRecords();

        // Obtener identificador del cliente
        var clientId = GetClientIdentifier(context);

        // Verificar si esta en whitelist
        if (_options.WhitelistedIps.Contains(clientId))
        {
            await _next(context);
            return;
        }

        // Obtener limite aplicable
        var limit = GetApplicableLimit(context);

        // Verificar y actualizar contador
        var clientInfo = _clientRequests.GetOrAdd(clientId, _ => new ClientRequestInfo());

        bool isLimitExceeded;
        int requestCount;
        int retryAfterSeconds;
        var now = DateTime.UtcNow;

        lock (clientInfo)
        {
            // Resetear contador si paso la ventana de tiempo
            if (now - clientInfo.WindowStart > TimeSpan.FromSeconds(limit.WindowSeconds))
            {
                clientInfo.RequestCount = 0;
                clientInfo.WindowStart = now;
            }

            clientInfo.RequestCount++;
            requestCount = clientInfo.RequestCount;
            isLimitExceeded = clientInfo.RequestCount > limit.MaxRequests;
            retryAfterSeconds = (int)(clientInfo.WindowStart.AddSeconds(limit.WindowSeconds) - now).TotalSeconds;
        }

        // Verificar si excede el limite (fuera del lock)
        if (isLimitExceeded)
        {
            _logger.LogWarning(
                "Rate limit excedido para cliente {ClientId}. Requests: {Count}/{Max} en {Window}s. Path: {Path}",
                clientId, requestCount, limit.MaxRequests, limit.WindowSeconds, context.Request.Path);

            // Agregar headers informativos
            context.Response.Headers["X-RateLimit-Limit"] = limit.MaxRequests.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";
            context.Response.Headers["X-RateLimit-Reset"] = retryAfterSeconds.ToString();
            context.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Demasiadas solicitudes",
                message = $"Ha excedido el limite de {limit.MaxRequests} requests por {limit.WindowSeconds} segundos",
                retryAfter = retryAfterSeconds
            };

            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        // Agregar headers de rate limit
        context.Response.Headers["X-RateLimit-Limit"] = limit.MaxRequests.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = (limit.MaxRequests - requestCount).ToString();
        context.Response.Headers["X-RateLimit-Reset"] = retryAfterSeconds.ToString();

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Prioridad: Usuario autenticado > IP
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.Identity.Name ?? "anonymous";
            return $"user:{userId}";
        }

        // Obtener IP real (considerando proxies)
        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        // Tomar solo la primera IP si hay multiples
        if (ip.Contains(','))
            ip = ip.Split(',')[0].Trim();

        return $"ip:{ip}";
    }

    private RateLimitRule GetApplicableLimit(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var method = context.Request.Method;

        // Buscar regla especifica para el endpoint
        foreach (var rule in _options.EndpointRules)
        {
            if (path.StartsWith(rule.PathPrefix.ToLower()) &&
                (string.IsNullOrEmpty(rule.Method) || rule.Method.Equals(method, StringComparison.OrdinalIgnoreCase)))
            {
                return rule;
            }
        }

        // Limites por tipo de endpoint
        if (path.Contains("/auth/login"))
        {
            return new RateLimitRule { MaxRequests = 5, WindowSeconds = 60 }; // 5 intentos por minuto
        }

        if (path.Contains("/auth/"))
        {
            return new RateLimitRule { MaxRequests = 20, WindowSeconds = 60 }; // 20 por minuto para auth
        }

        if (path.Contains("/exportar") || path.Contains("/excel") || path.Contains("/pdf"))
        {
            return new RateLimitRule { MaxRequests = 10, WindowSeconds = 60 }; // 10 por minuto para exports
        }

        // Limite por defecto
        return new RateLimitRule
        {
            MaxRequests = _options.DefaultMaxRequests,
            WindowSeconds = _options.DefaultWindowSeconds
        };
    }

    private void CleanupOldRecords()
    {
        var now = DateTime.UtcNow;

        // Limpiar cada 5 minutos
        if ((now - _lastCleanup).TotalMinutes < 5)
            return;

        lock (_cleanupLock)
        {
            if ((now - _lastCleanup).TotalMinutes < 5)
                return;

            _lastCleanup = now;

            var expiredKeys = _clientRequests
                .Where(x => (now - x.Value.WindowStart).TotalMinutes > 10)
                .Select(x => x.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _clientRequests.TryRemove(key, out _);
            }
        }
    }

    private class ClientRequestInfo
    {
        public int RequestCount { get; set; }
        public DateTime WindowStart { get; set; } = DateTime.UtcNow;
    }
}

/// <summary>
/// Opciones de Rate Limiting
/// </summary>
public class RateLimitingOptions
{
    /// <summary>
    /// Maximo de requests por defecto
    /// </summary>
    public int DefaultMaxRequests { get; set; } = 100;

    /// <summary>
    /// Ventana de tiempo en segundos por defecto
    /// </summary>
    public int DefaultWindowSeconds { get; set; } = 60;

    /// <summary>
    /// IPs en whitelist (sin limite)
    /// </summary>
    public HashSet<string> WhitelistedIps { get; set; } = new()
    {
        "ip:127.0.0.1",
        "ip:::1"
    };

    /// <summary>
    /// Reglas especificas por endpoint
    /// </summary>
    public List<RateLimitRule> EndpointRules { get; set; } = new();
}

/// <summary>
/// Regla de rate limiting
/// </summary>
public class RateLimitRule
{
    /// <summary>
    /// Prefijo del path
    /// </summary>
    public string PathPrefix { get; set; } = "/";

    /// <summary>
    /// Metodo HTTP (vacio = todos)
    /// </summary>
    public string Method { get; set; } = "";

    /// <summary>
    /// Maximo de requests
    /// </summary>
    public int MaxRequests { get; set; } = 100;

    /// <summary>
    /// Ventana de tiempo en segundos
    /// </summary>
    public int WindowSeconds { get; set; } = 60;
}

/// <summary>
/// Extension para registrar el middleware
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(
        this IApplicationBuilder builder,
        RateLimitingOptions? options = null)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>(options ?? new RateLimitingOptions());
    }

    public static IApplicationBuilder UseRateLimiting(
        this IApplicationBuilder builder,
        Action<RateLimitingOptions> configureOptions)
    {
        var options = new RateLimitingOptions();
        configureOptions(options);
        return builder.UseMiddleware<RateLimitingMiddleware>(options);
    }
}
