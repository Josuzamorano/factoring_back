using Factoring.API.Middleware;
using Factoring.Core.DTOs;
using Factoring.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Factoring.API.Controllers;

/// <summary>
/// Controller para gestion de auditoria y trazabilidad
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditoriaController : ControllerBase
{
    private readonly AuditService _auditService;
    private readonly ILogger<AuditoriaController> _logger;

    public AuditoriaController(
        AuditService auditService,
        ILogger<AuditoriaController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    // ==================== CONSULTAS ====================

    /// <summary>
    /// Busqueda paginada de registros de auditoria
    /// </summary>
    [HttpPost("buscar")]
    [RequierePermiso("auditoria.consultar")]
    public async Task<ActionResult<AuditoriaPaginadaDto>> Buscar([FromBody] AuditoriaFiltrosDto filtros)
    {
        try
        {
            var resultado = await _auditService.BuscarPaginadoAsync(filtros);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en busqueda de auditoria");
            return StatusCode(500, new { error = "Error al buscar registros de auditoria" });
        }
    }

    /// <summary>
    /// Obtiene un registro de auditoria por ID
    /// </summary>
    [HttpGet("{id:long}")]
    [RequierePermiso("auditoria.consultar")]
    public async Task<ActionResult<AuditoriaRegistroDto>> GetById(long id)
    {
        try
        {
            var registro = await _auditService.GetByIdAsync(id);
            if (registro == null)
                return NotFound(new { error = "Registro de auditoria no encontrado" });

            return Ok(registro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener registro de auditoria {Id}", id);
            return StatusCode(500, new { error = "Error al obtener registro de auditoria" });
        }
    }

    /// <summary>
    /// Obtiene timeline de actividad de una entidad
    /// </summary>
    [HttpGet("timeline/{entidad}/{entidadId}")]
    [RequierePermiso("auditoria.consultar")]
    public async Task<ActionResult<TimelineEntidadDto>> GetTimeline(string entidad, string entidadId)
    {
        try
        {
            var timeline = await _auditService.GetTimelineEntidadAsync(entidad, entidadId);
            return Ok(timeline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener timeline de {Entidad}/{EntidadId}", entidad, entidadId);
            return StatusCode(500, new { error = "Error al obtener timeline" });
        }
    }

    /// <summary>
    /// Obtiene registros de auditoria por usuario
    /// </summary>
    [HttpGet("usuario/{usuario}")]
    [RequierePermiso("auditoria.consultar")]
    public async Task<ActionResult<IEnumerable<AuditoriaRegistroDto>>> GetByUsuario(
        string usuario,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        try
        {
            var registros = await _auditService.GetByUsuarioAsync(usuario, desde, hasta);
            return Ok(registros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener auditoria de usuario {Usuario}", usuario);
            return StatusCode(500, new { error = "Error al obtener registros de auditoria" });
        }
    }

    /// <summary>
    /// Obtiene registros de auditoria criticos
    /// </summary>
    [HttpGet("criticos")]
    [RequierePermiso("auditoria.consultar")]
    public async Task<ActionResult<IEnumerable<AuditoriaRegistroDto>>> GetCriticos(
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        try
        {
            var registros = await _auditService.GetCriticosAsync(desde, hasta);
            return Ok(registros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener registros criticos");
            return StatusCode(500, new { error = "Error al obtener registros criticos" });
        }
    }

    // ==================== REPORTES Y ESTADISTICAS ====================

    /// <summary>
    /// Obtiene resumen de auditoria (dashboard)
    /// </summary>
    [HttpGet("resumen")]
    [RequierePermiso("auditoria.consultar")]
    public async Task<ActionResult<AuditoriaResumenDto>> GetResumen()
    {
        try
        {
            var resumen = await _auditService.GetResumenAsync();
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen de auditoria");
            return StatusCode(500, new { error = "Error al obtener resumen" });
        }
    }

    /// <summary>
    /// Obtiene estadisticas detalladas por periodo
    /// </summary>
    [HttpGet("estadisticas")]
    [RequierePermiso("auditoria.consultar")]
    public async Task<ActionResult<EstadisticasAuditoriaDto>> GetEstadisticas(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta)
    {
        try
        {
            var estadisticas = await _auditService.GetEstadisticasAsync(desde, hasta);
            return Ok(estadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadisticas de auditoria");
            return StatusCode(500, new { error = "Error al obtener estadisticas" });
        }
    }

    /// <summary>
    /// Obtiene reporte de actividad de usuario
    /// </summary>
    [HttpGet("reporte-usuario/{usuario}")]
    [RequierePermiso("auditoria.consultar")]
    public async Task<ActionResult<ReporteActividadUsuarioDto>> GetReporteUsuario(
        string usuario,
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta)
    {
        try
        {
            var reporte = await _auditService.GetReporteUsuarioAsync(usuario, desde, hasta);
            return Ok(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reporte de usuario {Usuario}", usuario);
            return StatusCode(500, new { error = "Error al obtener reporte de usuario" });
        }
    }

    // ==================== ALERTAS ====================

    /// <summary>
    /// Obtiene actividad inusual de un usuario
    /// </summary>
    [HttpGet("actividad-inusual/{usuario}")]
    [RequierePermiso("auditoria.alertas")]
    public async Task<ActionResult<IEnumerable<AuditoriaRegistroDto>>> GetActividadInusual(
        string usuario,
        [FromQuery] int dias = 7)
    {
        try
        {
            var actividad = await _auditService.GetActividadInusualAsync(usuario, dias);
            return Ok(actividad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener actividad inusual de {Usuario}", usuario);
            return StatusCode(500, new { error = "Error al obtener actividad inusual" });
        }
    }

    /// <summary>
    /// Detecta multiples fallos de login
    /// </summary>
    [HttpGet("detectar-fallos-login")]
    [RequierePermiso("auditoria.alertas")]
    public async Task<ActionResult<object>> DetectarFallosLogin(
        [FromQuery] string? usuario = null,
        [FromQuery] string? ip = null,
        [FromQuery] int maxIntentos = 5,
        [FromQuery] int minutos = 15)
    {
        try
        {
            var detectado = await _auditService.DetectarFallosLoginAsync(usuario, ip, maxIntentos, minutos);
            return Ok(new
            {
                detectado,
                mensaje = detectado
                    ? $"Se detectaron mas de {maxIntentos} fallos de login en los ultimos {minutos} minutos"
                    : "No se detectaron anomalias"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al detectar fallos de login");
            return StatusCode(500, new { error = "Error al detectar fallos de login" });
        }
    }

    // ==================== RETENCION ====================

    /// <summary>
    /// Obtiene configuracion de retencion
    /// </summary>
    [HttpGet("retencion/configuracion")]
    [RequiereAdmin]
    public async Task<ActionResult<ConfiguracionRetencionDto>> GetConfiguracionRetencion()
    {
        try
        {
            var config = await _auditService.GetConfiguracionRetencionAsync();
            if (config == null)
                return NotFound(new { error = "No se encontro configuracion de retencion" });

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuracion de retencion");
            return StatusCode(500, new { error = "Error al obtener configuracion" });
        }
    }

    /// <summary>
    /// Actualiza configuracion de retencion
    /// </summary>
    [HttpPut("retencion/configuracion")]
    [RequiereAdmin]
    public async Task<ActionResult> ActualizarConfiguracionRetencion([FromBody] ActualizarRetencionRequest request)
    {
        try
        {
            // Validaciones
            if (request.DiasRetencionGeneral < 30)
                return BadRequest(new { error = "La retencion general minima es de 30 dias" });

            if (request.DiasRetencionCriticos < 90)
                return BadRequest(new { error = "La retencion de eventos criticos minima es de 90 dias" });

            if (request.DiasRetencionCriticos < request.DiasRetencionGeneral)
                return BadRequest(new { error = "La retencion de eventos criticos debe ser mayor a la general" });

            await _auditService.ActualizarConfiguracionRetencionAsync(request);
            return Ok(new { mensaje = "Configuracion actualizada correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar configuracion de retencion");
            return StatusCode(500, new { error = "Error al actualizar configuracion" });
        }
    }

    /// <summary>
    /// Ejecuta limpieza manual de registros antiguos
    /// </summary>
    [HttpPost("retencion/ejecutar-limpieza")]
    [RequiereAdmin]
    public async Task<ActionResult<ResultadoLimpiezaDto>> EjecutarLimpieza()
    {
        try
        {
            _logger.LogWarning("Ejecutando limpieza manual de auditoria por {Usuario}", User.Identity?.Name);

            var resultado = await _auditService.EjecutarLimpiezaAsync();

            if (!resultado.Exitoso)
                return BadRequest(new { error = resultado.Error });

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar limpieza de auditoria");
            return StatusCode(500, new { error = "Error al ejecutar limpieza" });
        }
    }

    // ==================== EXPORTACION ====================

    /// <summary>
    /// Exporta registros de auditoria a CSV
    /// </summary>
    [HttpPost("exportar/csv")]
    [RequierePermiso("auditoria.exportar")]
    public async Task<ActionResult> ExportarCsv([FromBody] AuditoriaFiltrosDto filtros)
    {
        try
        {
            // Limitar registros para exportacion
            filtros.RegistrosPorPagina = 10000;
            filtros.Pagina = 1;

            var resultado = await _auditService.BuscarPaginadoAsync(filtros);

            // Generar CSV
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,Fecha,Usuario,Entidad,EntidadID,Accion,Modulo,Nivel,IP,Descripcion");

            foreach (var reg in resultado.Registros)
            {
                csv.AppendLine($"{reg.Id},{reg.FechaHora:yyyy-MM-dd HH:mm:ss},\"{reg.Usuario}\"," +
                    $"\"{reg.Entidad}\",\"{reg.EntidadId}\",\"{reg.Accion}\"," +
                    $"\"{reg.Modulo}\",\"{reg.NivelCriticidad}\",\"{reg.Ip}\"," +
                    $"\"{reg.Descripcion?.Replace("\"", "\"\"")}\"");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"auditoria_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar auditoria a CSV");
            return StatusCode(500, new { error = "Error al exportar auditoria" });
        }
    }

    /// <summary>
    /// Obtiene modulos disponibles para filtros
    /// </summary>
    [HttpGet("modulos")]
    public ActionResult<IEnumerable<string>> GetModulos()
    {
        var modulos = new[]
        {
            "OPERACIONES", "CLIENTES", "DEUDORES", "DOCUMENTOS",
            "TESORERIA", "COBRANZA", "LEGAL", "FINANZAS",
            "LINEAS_CREDITO", "RECAUDACION", "AUDITORIA",
            "SEGURIDAD", "CONFIGURACION"
        };

        return Ok(modulos);
    }

    /// <summary>
    /// Obtiene niveles de criticidad disponibles
    /// </summary>
    [HttpGet("niveles-criticidad")]
    public ActionResult<IEnumerable<object>> GetNivelesCriticidad()
    {
        var niveles = new[]
        {
            new { codigo = "LOW", nombre = "Bajo", color = "success" },
            new { codigo = "MEDIUM", nombre = "Medio", color = "info" },
            new { codigo = "HIGH", nombre = "Alto", color = "warning" },
            new { codigo = "CRITICAL", nombre = "Critico", color = "error" }
        };

        return Ok(niveles);
    }

    /// <summary>
    /// Obtiene acciones disponibles para filtros
    /// </summary>
    [HttpGet("acciones")]
    public ActionResult<IEnumerable<object>> GetAcciones()
    {
        var acciones = new[]
        {
            new { codigo = "INSERT", nombre = "Creacion" },
            new { codigo = "UPDATE", nombre = "Actualizacion" },
            new { codigo = "DELETE", nombre = "Eliminacion" },
            new { codigo = "VIEW", nombre = "Consulta" },
            new { codigo = "EXPORT", nombre = "Exportacion" },
            new { codigo = "APPROVE", nombre = "Aprobacion" },
            new { codigo = "REJECT", nombre = "Rechazo" },
            new { codigo = "STATUS_CHANGE", nombre = "Cambio de estado" },
            new { codigo = "LOGIN", nombre = "Inicio de sesion" },
            new { codigo = "LOGOUT", nombre = "Cierre de sesion" },
            new { codigo = "LOGIN_FAILED", nombre = "Fallo de login" }
        };

        return Ok(acciones);
    }
}
