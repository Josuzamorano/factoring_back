namespace Factoring.API.Middleware;

/// <summary>
/// Middleware para agregar headers de seguridad HTTP
/// Protege contra XSS, clickjacking, sniffing y otros ataques
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersOptions _options;

    public SecurityHeadersMiddleware(RequestDelegate next, SecurityHeadersOptions? options = null)
    {
        _next = next;
        _options = options ?? new SecurityHeadersOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Content-Security-Policy - Protege contra XSS
        if (_options.EnableContentSecurityPolicy)
        {
            context.Response.Headers["Content-Security-Policy"] = _options.ContentSecurityPolicy;
        }

        // X-Content-Type-Options - Previene MIME sniffing
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // X-Frame-Options - Protege contra clickjacking
        context.Response.Headers["X-Frame-Options"] = _options.FrameOptions;

        // X-XSS-Protection - Proteccion XSS del navegador (legacy pero util)
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

        // Referrer-Policy - Controla informacion enviada en Referer
        context.Response.Headers["Referrer-Policy"] = _options.ReferrerPolicy;

        // Permissions-Policy - Controla acceso a APIs del navegador
        context.Response.Headers["Permissions-Policy"] = _options.PermissionsPolicy;

        // Strict-Transport-Security (HSTS) - Solo si esta en produccion y HTTPS
        if (_options.EnableHsts && context.Request.IsHttps)
        {
            context.Response.Headers["Strict-Transport-Security"] =
                $"max-age={_options.HstsMaxAgeSeconds}; includeSubDomains";
        }

        // Cache-Control para contenido sensible
        if (_options.EnableNoCacheForApi && context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "0";
        }

        // Remover headers que revelan informacion del servidor
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");
            context.Response.Headers.Remove("X-AspNet-Version");
            return Task.CompletedTask;
        });

        await _next(context);
    }
}

/// <summary>
/// Opciones de configuracion para headers de seguridad
/// </summary>
public class SecurityHeadersOptions
{
    /// <summary>
    /// Habilita Content-Security-Policy
    /// </summary>
    public bool EnableContentSecurityPolicy { get; set; } = true;

    /// <summary>
    /// Politica de seguridad de contenido
    /// </summary>
    public string ContentSecurityPolicy { get; set; } =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' data:; " +
        "connect-src 'self'; " +
        "frame-ancestors 'self';";

    /// <summary>
    /// Opciones de X-Frame-Options
    /// </summary>
    public string FrameOptions { get; set; } = "DENY";

    /// <summary>
    /// Politica de referrer
    /// </summary>
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

    /// <summary>
    /// Politica de permisos
    /// </summary>
    public string PermissionsPolicy { get; set; } =
        "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";

    /// <summary>
    /// Habilita HSTS
    /// </summary>
    public bool EnableHsts { get; set; } = true;

    /// <summary>
    /// Tiempo de vida de HSTS en segundos (1 año por defecto)
    /// </summary>
    public int HstsMaxAgeSeconds { get; set; } = 31536000;

    /// <summary>
    /// Deshabilita cache para endpoints de API
    /// </summary>
    public bool EnableNoCacheForApi { get; set; } = true;
}

/// <summary>
/// Extension para registrar el middleware
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder builder,
        SecurityHeadersOptions? options = null)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>(options ?? new SecurityHeadersOptions());
    }

    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder builder,
        Action<SecurityHeadersOptions> configureOptions)
    {
        var options = new SecurityHeadersOptions();
        configureOptions(options);
        return builder.UseMiddleware<SecurityHeadersMiddleware>(options);
    }
}
