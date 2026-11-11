using System.Text;
using System.Text.RegularExpressions;

namespace Factoring.API.Middleware;

/// <summary>
/// Middleware para validacion y sanitizacion de input
/// Protege contra XSS, SQL Injection y otros ataques de inyeccion
/// </summary>
public class InputValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly InputValidationOptions _options;
    private readonly ILogger<InputValidationMiddleware> _logger;

    // Patrones peligrosos
    private static readonly Regex SqlInjectionPattern = new(
        @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|TRUNCATE|ALTER|CREATE|EXEC|EXECUTE|UNION|DECLARE|CAST|CONVERT)\b.*\b(FROM|INTO|TABLE|DATABASE|WHERE|SET|VALUES|OR|AND)\b)|('.*--)|(--)|(;.*\bDROP\b)|(\bXP_\w+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex XssPattern = new(
        @"<script[^>]*>[\s\S]*?</script>|javascript:|on\w+\s*=|<iframe|<embed|<object|<link.*href|<meta.*http-equiv|expression\s*\(|url\s*\(",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex PathTraversalPattern = new(
        @"\.\./|\.\.\\|%2e%2e%2f|%2e%2e/|\.%2e/|%2e\./",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex CommandInjectionPattern = new(
        @"[;&|`$]|\$\(|`.*`|\|\||&&",
        RegexOptions.Compiled);

    public InputValidationMiddleware(
        RequestDelegate next,
        InputValidationOptions options,
        ILogger<InputValidationMiddleware> logger)
    {
        _next = next;
        _options = options;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Validar query string
        if (_options.ValidateQueryString && context.Request.QueryString.HasValue)
        {
            var queryString = context.Request.QueryString.Value;
            if (!string.IsNullOrEmpty(queryString))
            {
                var (isValid, threatType) = ValidateInput(queryString, "QueryString");
                if (!isValid)
                {
                    await HandleThreatDetected(context, threatType, queryString);
                    return;
                }
            }
        }

        // Validar headers sensibles
        if (_options.ValidateHeaders)
        {
            foreach (var header in _options.HeadersToValidate)
            {
                if (context.Request.Headers.TryGetValue(header, out var headerValue))
                {
                    var (isValid, threatType) = ValidateInput(headerValue.ToString(), $"Header:{header}");
                    if (!isValid)
                    {
                        await HandleThreatDetected(context, threatType, headerValue.ToString());
                        return;
                    }
                }
            }
        }

        // Validar body para POST/PUT/PATCH
        if (_options.ValidateBody && HttpMethods.IsPost(context.Request.Method) ||
            HttpMethods.IsPut(context.Request.Method) ||
            HttpMethods.IsPatch(context.Request.Method))
        {
            // Solo validar si es JSON o form
            var contentType = context.Request.ContentType?.ToLower() ?? "";
            if (contentType.Contains("application/json") || contentType.Contains("application/x-www-form-urlencoded"))
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true);

                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (!string.IsNullOrEmpty(body) && body.Length <= _options.MaxBodySizeToValidate)
                {
                    var (isValid, threatType) = ValidateInput(body, "Body");
                    if (!isValid)
                    {
                        await HandleThreatDetected(context, threatType, body.Length > 200 ? body.Substring(0, 200) + "..." : body);
                        return;
                    }
                }
            }
        }

        // Validar path
        if (_options.ValidatePath)
        {
            var path = context.Request.Path.Value;
            if (!string.IsNullOrEmpty(path))
            {
                if (PathTraversalPattern.IsMatch(path))
                {
                    await HandleThreatDetected(context, "Path Traversal", path);
                    return;
                }
            }
        }

        await _next(context);
    }

    private (bool IsValid, string ThreatType) ValidateInput(string input, string source)
    {
        if (string.IsNullOrEmpty(input))
            return (true, string.Empty);

        // SQL Injection
        if (_options.DetectSqlInjection && SqlInjectionPattern.IsMatch(input))
        {
            _logger.LogWarning("Posible SQL Injection detectado en {Source}", source);
            return (false, "SQL Injection");
        }

        // XSS
        if (_options.DetectXss && XssPattern.IsMatch(input))
        {
            _logger.LogWarning("Posible XSS detectado en {Source}", source);
            return (false, "XSS");
        }

        // Command Injection
        if (_options.DetectCommandInjection && CommandInjectionPattern.IsMatch(input))
        {
            // Solo alertar si parece sospechoso en contexto
            if (input.Contains("rm ") || input.Contains("del ") || input.Contains("format "))
            {
                _logger.LogWarning("Posible Command Injection detectado en {Source}", source);
                return (false, "Command Injection");
            }
        }

        // Path Traversal
        if (_options.DetectPathTraversal && PathTraversalPattern.IsMatch(input))
        {
            _logger.LogWarning("Posible Path Traversal detectado en {Source}", source);
            return (false, "Path Traversal");
        }

        return (true, string.Empty);
    }

    private async Task HandleThreatDetected(HttpContext context, string threatType, string input)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var user = context.User?.Identity?.Name ?? "anonymous";

        _logger.LogError(
            "Amenaza detectada: {ThreatType}. IP: {ClientIp}, Usuario: {User}, Path: {Path}, Input: {Input}",
            threatType, clientIp, user, context.Request.Path, input.Length > 100 ? input.Substring(0, 100) : input);

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Solicitud rechazada",
            message = "La solicitud contiene contenido potencialmente peligroso",
            threatType = _options.RevealThreatType ? threatType : null
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Opciones de validacion de input
/// </summary>
public class InputValidationOptions
{
    /// <summary>
    /// Validar query string
    /// </summary>
    public bool ValidateQueryString { get; set; } = true;

    /// <summary>
    /// Validar body de request
    /// </summary>
    public bool ValidateBody { get; set; } = true;

    /// <summary>
    /// Validar headers
    /// </summary>
    public bool ValidateHeaders { get; set; } = true;

    /// <summary>
    /// Validar path
    /// </summary>
    public bool ValidatePath { get; set; } = true;

    /// <summary>
    /// Detectar SQL Injection
    /// </summary>
    public bool DetectSqlInjection { get; set; } = true;

    /// <summary>
    /// Detectar XSS
    /// </summary>
    public bool DetectXss { get; set; } = true;

    /// <summary>
    /// Detectar Command Injection
    /// </summary>
    public bool DetectCommandInjection { get; set; } = true;

    /// <summary>
    /// Detectar Path Traversal
    /// </summary>
    public bool DetectPathTraversal { get; set; } = true;

    /// <summary>
    /// Revelar tipo de amenaza en respuesta (solo desarrollo)
    /// </summary>
    public bool RevealThreatType { get; set; } = false;

    /// <summary>
    /// Tamaño maximo de body a validar (bytes)
    /// </summary>
    public int MaxBodySizeToValidate { get; set; } = 1024 * 1024; // 1MB

    /// <summary>
    /// Headers a validar
    /// </summary>
    public List<string> HeadersToValidate { get; set; } = new()
    {
        "User-Agent",
        "Referer",
        "X-Forwarded-For"
    };
}

/// <summary>
/// Extension para registrar el middleware
/// </summary>
public static class InputValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseInputValidation(
        this IApplicationBuilder builder,
        InputValidationOptions? options = null)
    {
        return builder.UseMiddleware<InputValidationMiddleware>(options ?? new InputValidationOptions());
    }

    public static IApplicationBuilder UseInputValidation(
        this IApplicationBuilder builder,
        Action<InputValidationOptions> configureOptions)
    {
        var options = new InputValidationOptions();
        configureOptions(options);
        return builder.UseMiddleware<InputValidationMiddleware>(options);
    }
}
