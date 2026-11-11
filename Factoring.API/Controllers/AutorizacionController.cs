using Factoring.API.Middleware;
using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using Factoring.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Factoring.API.Controllers;

/// <summary>
/// Controlador de Autorizacion Granular
/// - Gestion de roles y permisos
/// - Restricciones por monto
/// - Flujo de aprobaciones (4 ojos)
/// - Delegacion de permisos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
[SwaggerTag("Gestion de autorizacion, roles y permisos")]
public class AutorizacionController : ControllerBase
{
    private readonly AutorizacionService _autorizacionService;

    public AutorizacionController(AutorizacionService autorizacionService)
    {
        _autorizacionService = autorizacionService;
    }

    private string GetUsuarioActual() => User.Identity?.Name ?? "";

    // ==================== MODULOS Y ACCIONES ====================

    /// <summary>
    /// Obtener todos los modulos con sus acciones
    /// </summary>
    [HttpGet("modulos")]
    [SwaggerOperation(Summary = "Listar modulos", Description = "Obtiene todos los modulos y acciones disponibles")]
    [ProducesResponseType(typeof(IEnumerable<ModuloDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ModuloDto>>> GetModulos()
    {
        var modulos = await _autorizacionService.GetModulosConAccionesAsync();
        return Ok(modulos);
    }

    // ==================== ROLES ====================

    /// <summary>
    /// Listar todos los roles
    /// </summary>
    [HttpGet("roles")]
    [SwaggerOperation(Summary = "Listar roles", Description = "Obtiene todos los roles del sistema")]
    [ProducesResponseType(typeof(IEnumerable<RolDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RolDto>>> GetRoles()
    {
        var roles = await _autorizacionService.GetRolesAsync();
        return Ok(roles);
    }

    /// <summary>
    /// Obtener rol con sus permisos
    /// </summary>
    [HttpGet("roles/{rolId}")]
    [SwaggerOperation(Summary = "Detalle de rol", Description = "Obtiene un rol con todos sus permisos")]
    [ProducesResponseType(typeof(RolDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RolDto>> GetRol(int rolId)
    {
        var rol = await _autorizacionService.GetRolConPermisosAsync(rolId);
        if (rol == null) return NotFound();
        return Ok(rol);
    }

    /// <summary>
    /// Crear nuevo rol
    /// </summary>
    [HttpPost("roles")]
    [RequiereAdmin]
    [SwaggerOperation(Summary = "Crear rol", Description = "Crea un nuevo rol con permisos")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<ActionResult<int>> CrearRol([FromBody] CrearRolRequest request)
    {
        var rolId = await _autorizacionService.CrearRolAsync(request, GetUsuarioActual());
        return CreatedAtAction(nameof(GetRol), new { rolId }, rolId);
    }

    /// <summary>
    /// Actualizar rol
    /// </summary>
    [HttpPut("roles/{rolId}")]
    [RequiereAdmin]
    [SwaggerOperation(Summary = "Actualizar rol", Description = "Actualiza un rol y sus permisos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ActualizarRol(int rolId, [FromBody] CrearRolRequest request)
    {
        await _autorizacionService.ActualizarRolAsync(rolId, request);
        return Ok(new { mensaje = "Rol actualizado" });
    }

    /// <summary>
    /// Eliminar rol
    /// </summary>
    [HttpDelete("roles/{rolId}")]
    [RequiereAdmin]
    [SwaggerOperation(Summary = "Eliminar rol", Description = "Elimina un rol (si no tiene usuarios)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EliminarRol(int rolId)
    {
        try
        {
            await _autorizacionService.EliminarRolAsync(rolId);
            return Ok(new { mensaje = "Rol eliminado" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // ==================== PERMISOS DE USUARIO ====================

    /// <summary>
    /// Obtener permisos de un usuario
    /// </summary>
    [HttpGet("usuarios/{usuarioLogin}/permisos")]
    [SwaggerOperation(Summary = "Permisos de usuario", Description = "Obtiene todos los permisos efectivos de un usuario")]
    [ProducesResponseType(typeof(UsuarioPermisosDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioPermisosDto>> GetPermisosUsuario(string usuarioLogin)
    {
        var permisos = await _autorizacionService.GetPermisosUsuarioAsync(usuarioLogin);
        if (permisos == null) return NotFound();
        return Ok(permisos);
    }

    /// <summary>
    /// Obtener mis permisos
    /// </summary>
    [HttpGet("mis-permisos")]
    [SwaggerOperation(Summary = "Mis permisos", Description = "Obtiene los permisos del usuario actual")]
    [ProducesResponseType(typeof(UsuarioPermisosDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsuarioPermisosDto>> GetMisPermisos()
    {
        var permisos = await _autorizacionService.GetPermisosUsuarioAsync(GetUsuarioActual());
        return Ok(permisos);
    }

    /// <summary>
    /// Asignar rol a usuario
    /// </summary>
    [HttpPost("usuarios/asignar-rol")]
    [RequiereAdmin]
    [SwaggerOperation(Summary = "Asignar rol", Description = "Asigna un rol a un usuario")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AsignarRol([FromBody] AsignarRolRequest request)
    {
        await _autorizacionService.AsignarRolAsync(request, GetUsuarioActual());
        return Ok(new { mensaje = "Rol asignado" });
    }

    /// <summary>
    /// Revocar rol de usuario
    /// </summary>
    [HttpDelete("usuarios/{usuarioLogin}/roles/{rolId}")]
    [RequiereAdmin]
    [SwaggerOperation(Summary = "Revocar rol", Description = "Revoca un rol de un usuario")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RevocarRol(string usuarioLogin, int rolId)
    {
        await _autorizacionService.RevocarRolAsync(usuarioLogin, rolId);
        return Ok(new { mensaje = "Rol revocado" });
    }

    /// <summary>
    /// Asignar permiso especial a usuario
    /// </summary>
    [HttpPost("usuarios/asignar-permiso")]
    [RequiereAdmin]
    [SwaggerOperation(Summary = "Asignar permiso especial", Description = "Asigna un permiso especifico a un usuario")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AsignarPermisoUsuario([FromBody] AsignarPermisoUsuarioRequest request)
    {
        await _autorizacionService.AsignarPermisoUsuarioAsync(request, GetUsuarioActual());
        return Ok(new { mensaje = "Permiso asignado" });
    }

    /// <summary>
    /// Revocar permiso especial de usuario
    /// </summary>
    [HttpDelete("usuarios/permisos/{permisoId}")]
    [RequiereAdmin]
    [SwaggerOperation(Summary = "Revocar permiso especial", Description = "Revoca un permiso especifico de un usuario")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RevocarPermisoUsuario(int permisoId)
    {
        await _autorizacionService.RevocarPermisoUsuarioAsync(permisoId);
        return Ok(new { mensaje = "Permiso revocado" });
    }

    // ==================== VERIFICACION ====================

    /// <summary>
    /// Verificar si tengo un permiso
    /// </summary>
    [HttpPost("verificar")]
    [SwaggerOperation(Summary = "Verificar permiso", Description = "Verifica si el usuario actual tiene un permiso")]
    [ProducesResponseType(typeof(VerificarPermisoResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<VerificarPermisoResponse>> VerificarPermiso([FromBody] VerificarPermisoRequest request)
    {
        var resultado = await _autorizacionService.VerificarPermisoAsync(GetUsuarioActual(), request);
        return Ok(resultado);
    }

    // ==================== APROBACIONES (4 OJOS) ====================

    /// <summary>
    /// Listar solicitudes pendientes de aprobar
    /// </summary>
    [HttpGet("aprobaciones/pendientes")]
    [SwaggerOperation(Summary = "Solicitudes pendientes", Description = "Lista las solicitudes que el usuario puede aprobar")]
    [ProducesResponseType(typeof(IEnumerable<SolicitudAprobacionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SolicitudAprobacionDto>>> GetSolicitudesPendientes()
    {
        var solicitudes = await _autorizacionService.GetSolicitudesPendientesAsync(GetUsuarioActual());
        return Ok(solicitudes);
    }

    /// <summary>
    /// Crear solicitud de aprobacion
    /// </summary>
    [HttpPost("aprobaciones")]
    [SwaggerOperation(Summary = "Crear solicitud", Description = "Crea una nueva solicitud de aprobacion")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<ActionResult<int>> CrearSolicitud([FromBody] CrearSolicitudAprobacionRequest request)
    {
        var solicitudId = await _autorizacionService.CrearSolicitudAprobacionAsync(request, GetUsuarioActual(), 1);
        return CreatedAtAction(nameof(GetSolicitudesPendientes), new { }, solicitudId);
    }

    /// <summary>
    /// Aprobar o rechazar solicitud
    /// </summary>
    [HttpPost("aprobaciones/procesar")]
    [SwaggerOperation(Summary = "Procesar aprobacion", Description = "Aprueba o rechaza una solicitud")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcesarAprobacion([FromBody] ProcesarAprobacionRequest request)
    {
        var exito = await _autorizacionService.ProcesarAprobacionAsync(request, GetUsuarioActual());
        if (!exito)
        {
            return BadRequest(new { mensaje = "No se pudo procesar la aprobacion" });
        }
        return Ok(new { mensaje = request.Aprobar ? "Solicitud aprobada" : "Solicitud rechazada" });
    }

    /// <summary>
    /// Historial de una solicitud
    /// </summary>
    [HttpGet("aprobaciones/{solicitudId}/historial")]
    [SwaggerOperation(Summary = "Historial de aprobacion", Description = "Obtiene el historial de una solicitud")]
    [ProducesResponseType(typeof(IEnumerable<HistorialAprobacionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HistorialAprobacionDto>>> GetHistorialAprobacion(int solicitudId)
    {
        var historial = await _autorizacionService.GetHistorialAprobacionAsync(solicitudId);
        return Ok(historial);
    }

    // ==================== DELEGACIONES ====================

    /// <summary>
    /// Listar mis delegaciones
    /// </summary>
    [HttpGet("delegaciones")]
    [SwaggerOperation(Summary = "Mis delegaciones", Description = "Lista delegaciones recibidas y otorgadas")]
    [ProducesResponseType(typeof(IEnumerable<DelegacionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DelegacionDto>>> GetDelegaciones()
    {
        var delegaciones = await _autorizacionService.GetDelegacionesAsync(GetUsuarioActual());
        return Ok(delegaciones);
    }

    /// <summary>
    /// Crear delegacion de permisos
    /// </summary>
    [HttpPost("delegaciones")]
    [SwaggerOperation(Summary = "Crear delegacion", Description = "Delega permisos a otro usuario")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> CrearDelegacion([FromBody] CrearDelegacionRequest request)
    {
        try
        {
            var delegacionId = await _autorizacionService.CrearDelegacionAsync(request, GetUsuarioActual());
            return CreatedAtAction(nameof(GetDelegaciones), new { }, delegacionId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Cancelar delegacion
    /// </summary>
    [HttpDelete("delegaciones/{delegacionId}")]
    [SwaggerOperation(Summary = "Cancelar delegacion", Description = "Cancela una delegacion activa")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelarDelegacion(int delegacionId)
    {
        await _autorizacionService.CancelarDelegacionAsync(delegacionId, GetUsuarioActual());
        return Ok(new { mensaje = "Delegacion cancelada" });
    }

    // ==================== RESTRICCIONES POR MONTO ====================

    /// <summary>
    /// Listar restricciones por monto
    /// </summary>
    [HttpGet("restricciones-monto")]
    [SwaggerOperation(Summary = "Restricciones por monto", Description = "Lista todas las restricciones por monto")]
    [ProducesResponseType(typeof(IEnumerable<RestriccionMontoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RestriccionMontoDto>>> GetRestriccionesMonto()
    {
        var restricciones = await _autorizacionService.GetRestriccionesMontoAsync();
        return Ok(restricciones);
    }

    /// <summary>
    /// Crear restriccion por monto
    /// </summary>
    [HttpPost("restricciones-monto")]
    [RequiereAdmin]
    [SwaggerOperation(Summary = "Crear restriccion", Description = "Crea una nueva restriccion por monto")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<ActionResult<int>> CrearRestriccionMonto([FromBody] CrearRestriccionMontoRequest request)
    {
        var restriccionId = await _autorizacionService.CrearRestriccionMontoAsync(request);
        return CreatedAtAction(nameof(GetRestriccionesMonto), new { }, restriccionId);
    }

    // ==================== RESUMEN ====================

    /// <summary>
    /// Resumen de seguridad
    /// </summary>
    [HttpGet("resumen")]
    [SwaggerOperation(Summary = "Resumen seguridad", Description = "Obtiene resumen del estado de seguridad")]
    [ProducesResponseType(typeof(ResumenSeguridadDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumenSeguridadDto>> GetResumen()
    {
        var resumen = await _autorizacionService.GetResumenSeguridadAsync();
        return Ok(resumen);
    }
}
