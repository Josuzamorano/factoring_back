using Factoring.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Factoring.API.Middleware;

/// <summary>
/// Atributo para verificar permisos granulares en endpoints
/// </summary>
/// <example>
/// [RequierePermiso("operaciones.crear")]
/// [RequierePermiso("operaciones.aprobar", MontoParametro = "monto")]
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequierePermisoAttribute : Attribute, IAsyncAuthorizationFilter
{
    public string AccionCodigo { get; }
    public string? MontoParametro { get; set; }
    public string? SucursalParametro { get; set; }

    public RequierePermisoAttribute(string accionCodigo)
    {
        AccionCodigo = accionCodigo;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // Verificar autenticacion
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var usuarioLogin = user.Identity?.Name;
        if (string.IsNullOrEmpty(usuarioLogin))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Obtener servicio de autorizacion
        var autorizacionService = context.HttpContext.RequestServices.GetRequiredService<AutorizacionService>();

        // Obtener monto si se especifico
        decimal? monto = null;
        if (!string.IsNullOrEmpty(MontoParametro))
        {
            monto = ObtenerValorDecimal(context, MontoParametro);
        }

        // Obtener sucursal si se especifico
        int? sucursalId = null;
        if (!string.IsNullOrEmpty(SucursalParametro))
        {
            sucursalId = ObtenerValorInt(context, SucursalParametro);
        }

        // Si no se especifico sucursal, intentar obtener del claim
        if (!sucursalId.HasValue)
        {
            var sucursalClaim = user.Claims.FirstOrDefault(c => c.Type == "sucursal")?.Value;
            if (int.TryParse(sucursalClaim, out var suc) && suc > 0)
            {
                sucursalId = suc;
            }
        }

        // Verificar permiso
        var resultado = await autorizacionService.VerificarPermisoAsync(usuarioLogin, new Core.DTOs.VerificarPermisoRequest
        {
            AccionCodigo = AccionCodigo,
            Monto = monto,
            SucursalId = sucursalId
        });

        if (!resultado.Permitido)
        {
            context.Result = new ObjectResult(new
            {
                error = "Acceso denegado",
                mensaje = resultado.MotivoRechazo,
                accion = AccionCodigo
            })
            {
                StatusCode = 403
            };
            return;
        }

        // Si requiere aprobacion, agregar header para que el frontend lo maneje
        if (resultado.RequiereAprobacion)
        {
            context.HttpContext.Response.Headers.Append("X-Requiere-Aprobacion", "true");
            context.HttpContext.Response.Headers.Append("X-Nivel-Aprobacion", resultado.NivelAprobacionRequerido?.ToString() ?? "1");
        }

        // Guardar info de autorizacion en HttpContext para uso posterior
        context.HttpContext.Items["PermisoVerificado"] = resultado;
        context.HttpContext.Items["MontoMaximoPermitido"] = resultado.MontoMaximoPermitido;
    }

    private static decimal? ObtenerValorDecimal(AuthorizationFilterContext context, string parametro)
    {
        // Buscar en route values
        if (context.RouteData.Values.TryGetValue(parametro, out var routeValue))
        {
            if (decimal.TryParse(routeValue?.ToString(), out var decValue))
                return decValue;
        }

        // Buscar en query string
        if (context.HttpContext.Request.Query.TryGetValue(parametro, out var queryValue))
        {
            if (decimal.TryParse(queryValue.FirstOrDefault(), out var decValue))
                return decValue;
        }

        return null;
    }

    private static int? ObtenerValorInt(AuthorizationFilterContext context, string parametro)
    {
        // Buscar en route values
        if (context.RouteData.Values.TryGetValue(parametro, out var routeValue))
        {
            if (int.TryParse(routeValue?.ToString(), out var intValue))
                return intValue;
        }

        // Buscar en query string
        if (context.HttpContext.Request.Query.TryGetValue(parametro, out var queryValue))
        {
            if (int.TryParse(queryValue.FirstOrDefault(), out var intValue))
                return intValue;
        }

        return null;
    }
}

/// <summary>
/// Atributo para requerir rol de administrador
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiereAdminAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var perfilClaim = user.Claims.FirstOrDefault(c => c.Type == "perfil")?.Value;
        if (perfilClaim != "1") // Perfil 1 = Administrador
        {
            context.Result = new ForbidResult();
        }
    }
}

/// <summary>
/// Atributo para requerir nivel minimo de rol
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiereNivelAttribute : Attribute, IAsyncAuthorizationFilter
{
    public int NivelMinimo { get; }

    public RequiereNivelAttribute(int nivelMinimo)
    {
        NivelMinimo = nivelMinimo;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var usuarioLogin = user.Identity?.Name;
        if (string.IsNullOrEmpty(usuarioLogin))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var autorizacionRepository = context.HttpContext.RequestServices
            .GetRequiredService<Core.Interfaces.IAutorizacionRepository>();

        var roles = await autorizacionRepository.GetRolesUsuarioAsync(usuarioLogin);
        var rolesData = await autorizacionRepository.GetRolesAsync();

        var tieneNivel = roles.Any(r =>
        {
            var rol = rolesData.FirstOrDefault(rd => rd.Id == r.RolId);
            return rol != null && rol.Nivel <= NivelMinimo;
        });

        if (!tieneNivel)
        {
            context.Result = new ObjectResult(new
            {
                error = "Acceso denegado",
                mensaje = $"Se requiere nivel {NivelMinimo} o superior"
            })
            {
                StatusCode = 403
            };
        }
    }
}
