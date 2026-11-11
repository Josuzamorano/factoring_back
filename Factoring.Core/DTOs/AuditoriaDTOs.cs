namespace Factoring.Core.DTOs;

// ==================== CONSULTA DE AUDITORIA ====================

/// <summary>
/// Filtros para busqueda de auditoria
/// </summary>
public class AuditoriaFiltrosDto
{
    public string? Entidad { get; set; }
    public string? EntidadId { get; set; }
    public string? Usuario { get; set; }
    public string? Modulo { get; set; }
    public string? Accion { get; set; }
    public string? NivelCriticidad { get; set; }
    public string? Ip { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public string? TextoBusqueda { get; set; }
    public int Pagina { get; set; } = 1;
    public int RegistrosPorPagina { get; set; } = 50;
    public string OrdenarPor { get; set; } = "FechaHora";
    public bool Descendente { get; set; } = true;
}

/// <summary>
/// Registro de auditoria para visualizacion
/// </summary>
public class AuditoriaRegistroDto
{
    public long Id { get; set; }
    public string Entidad { get; set; } = string.Empty;
    public string EntidadId { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string AccionDescripcion { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string? UsuarioNombre { get; set; }
    public DateTime FechaHora { get; set; }
    public string? Ip { get; set; }
    public string? Maquina { get; set; }
    public string? Descripcion { get; set; }
    public string? Modulo { get; set; }
    public string NivelCriticidad { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public Dictionary<string, object>? ValoresAnteriores { get; set; }
    public Dictionary<string, object>? ValoresNuevos { get; set; }
    public List<CambioCampoDto>? CambiosDetallados { get; set; }
}

/// <summary>
/// Detalle de un campo cambiado
/// </summary>
public class CambioCampoDto
{
    public string Campo { get; set; } = string.Empty;
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
}

/// <summary>
/// Resultado paginado de auditoria
/// </summary>
public class AuditoriaPaginadaDto
{
    public List<AuditoriaRegistroDto> Registros { get; set; } = new();
    public int TotalRegistros { get; set; }
    public int Pagina { get; set; }
    public int TotalPaginas { get; set; }
    public int RegistrosPorPagina { get; set; }
}

// ==================== REPORTES DE AUDITORIA ====================

/// <summary>
/// Resumen de actividad de auditoria
/// </summary>
public class AuditoriaResumenDto
{
    public int TotalRegistros { get; set; }
    public int RegistrosHoy { get; set; }
    public int RegistrosSemana { get; set; }
    public int RegistrosMes { get; set; }
    public int EventosCriticos { get; set; }
    public int EventosAltos { get; set; }
    public DateTime? UltimoEvento { get; set; }
    public List<ActividadPorModuloDto> ActividadPorModulo { get; set; } = new();
    public List<ActividadPorUsuarioDto> UsuariosMasActivos { get; set; } = new();
    public List<ActividadPorAccionDto> AccionesMasFrecuentes { get; set; } = new();
    public List<ActividadPorDiaDto> TendenciaSemanal { get; set; } = new();
}

public class ActividadPorModuloDto
{
    public string Modulo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public int Criticos { get; set; }
}

public class ActividadPorUsuarioDto
{
    public string Usuario { get; set; } = string.Empty;
    public string? UsuarioNombre { get; set; }
    public int Cantidad { get; set; }
    public DateTime? UltimaActividad { get; set; }
}

public class ActividadPorAccionDto
{
    public string Accion { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class ActividadPorDiaDto
{
    public DateTime Fecha { get; set; }
    public int Cantidad { get; set; }
    public int Criticos { get; set; }
}

/// <summary>
/// Timeline de actividad de una entidad
/// </summary>
public class TimelineEntidadDto
{
    public string Entidad { get; set; } = string.Empty;
    public string EntidadId { get; set; } = string.Empty;
    public List<TimelineEventoDto> Eventos { get; set; } = new();
}

public class TimelineEventoDto
{
    public long AuditId { get; set; }
    public DateTime FechaHora { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string NivelCriticidad { get; set; } = string.Empty;
    public string? Icono { get; set; }
    public string? Color { get; set; }
}

/// <summary>
/// Reporte de actividad de usuario
/// </summary>
public class ReporteActividadUsuarioDto
{
    public string Usuario { get; set; } = string.Empty;
    public string? UsuarioNombre { get; set; }
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public int TotalAcciones { get; set; }
    public List<AccionPorModuloDto> AccionesPorModulo { get; set; } = new();
    public List<SesionUsuarioAuditoriaDto> Sesiones { get; set; } = new();
    public List<AuditoriaRegistroDto> UltimasAcciones { get; set; } = new();
}

public class AccionPorModuloDto
{
    public string Modulo { get; set; } = string.Empty;
    public int Creaciones { get; set; }
    public int Actualizaciones { get; set; }
    public int Eliminaciones { get; set; }
    public int Otros { get; set; }
}

public class SesionUsuarioAuditoriaDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string Ip { get; set; } = string.Empty;
    public int AccionesRealizadas { get; set; }
}

// ==================== CONFIGURACION DE RETENCION ====================

/// <summary>
/// Configuracion de retencion de datos de auditoria
/// </summary>
public class ConfiguracionRetencionDto
{
    public int Id { get; set; }
    public int DiasRetencionGeneral { get; set; }
    public int DiasRetencionCriticos { get; set; }
    public int DiasRetencionLogin { get; set; }
    public bool ArchivarAntesDeEliminar { get; set; }
    public string? RutaArchivo { get; set; }
    public bool Activo { get; set; }
    public DateTime? UltimaLimpieza { get; set; }
    public int? RegistrosEliminados { get; set; }
}

public class ActualizarRetencionRequest
{
    public int DiasRetencionGeneral { get; set; } = 365;
    public int DiasRetencionCriticos { get; set; } = 730;
    public int DiasRetencionLogin { get; set; } = 90;
    public bool ArchivarAntesDeEliminar { get; set; } = true;
}

/// <summary>
/// Resultado de proceso de limpieza
/// </summary>
public class ResultadoLimpiezaDto
{
    public DateTime FechaEjecucion { get; set; }
    public int RegistrosEliminados { get; set; }
    public int RegistrosArchivados { get; set; }
    public string? ArchivoGenerado { get; set; }
    public bool Exitoso { get; set; }
    public string? Error { get; set; }
}

// ==================== ALERTAS DE SEGURIDAD ====================

/// <summary>
/// Alerta de seguridad basada en auditoria
/// </summary>
public class AlertaAuditoriaDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string NivelSeveridad { get; set; } = string.Empty;
    public DateTime FechaDeteccion { get; set; }
    public string? Usuario { get; set; }
    public string? Ip { get; set; }
    public bool Revisada { get; set; }
    public string? RevisadaPor { get; set; }
    public DateTime? FechaRevision { get; set; }
    public List<long>? EventosRelacionados { get; set; }
}

/// <summary>
/// Tipos de alertas de seguridad
/// </summary>
public static class TipoAlertaAuditoria
{
    public const string MultiplesFallosLogin = "MULTIPLES_FALLOS_LOGIN";
    public const string AccesoHoraInusual = "ACCESO_HORA_INUSUAL";
    public const string CambioMasivoPermisos = "CAMBIO_MASIVO_PERMISOS";
    public const string ExportacionMasivaDatos = "EXPORTACION_MASIVA";
    public const string EliminacionesFrecuentes = "ELIMINACIONES_FRECUENTES";
    public const string AccesoIpDesconocida = "ACCESO_IP_DESCONOCIDA";
    public const string CambioConfiguracionCritica = "CAMBIO_CONFIG_CRITICA";
}

// ==================== ESTADISTICAS ====================

/// <summary>
/// Estadisticas de auditoria por periodo
/// </summary>
public class EstadisticasAuditoriaDto
{
    public DateTime PeriodoDesde { get; set; }
    public DateTime PeriodoHasta { get; set; }
    public int TotalEventos { get; set; }
    public int EventosPorDiaPromedio { get; set; }
    public int UsuariosUnicos { get; set; }
    public int IpsUnicas { get; set; }
    public Dictionary<string, int> EventosPorAccion { get; set; } = new();
    public Dictionary<string, int> EventosPorModulo { get; set; } = new();
    public Dictionary<string, int> EventosPorNivel { get; set; } = new();
    public List<HoraPicoDto> HorasPico { get; set; } = new();
}

public class HoraPicoDto
{
    public int Hora { get; set; }
    public int Cantidad { get; set; }
}

/// <summary>
/// Comparativa de periodos
/// </summary>
public class ComparativaAuditoriaDto
{
    public EstadisticasAuditoriaDto PeriodoActual { get; set; } = new();
    public EstadisticasAuditoriaDto PeriodoAnterior { get; set; } = new();
    public decimal VariacionPorcentual { get; set; }
    public string Tendencia { get; set; } = string.Empty; // Aumentando, Estable, Disminuyendo
}
