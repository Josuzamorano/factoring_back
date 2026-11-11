using Factoring.Core.DTOs;
using Factoring.Core.Entities;

namespace Factoring.Core.Interfaces;

/// <summary>
/// Repositorio para operaciones de auditoria
/// </summary>
public interface IAuditRepository
{
    // ==================== REGISTRO ====================

    /// <summary>
    /// Registra un evento de auditoria
    /// </summary>
    Task RegistrarAsync(AuditLog audit);

    // ==================== CONSULTAS BASICAS ====================

    /// <summary>
    /// Obtiene registros de auditoria por entidad
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByEntidadAsync(string entidad, string entidadId);

    /// <summary>
    /// Obtiene registros de auditoria por usuario
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByUsuarioAsync(string usuario, DateTime? desde = null, DateTime? hasta = null);

    /// <summary>
    /// Obtiene registros de auditoria por modulo
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByModuloAsync(string modulo, DateTime? desde = null, DateTime? hasta = null);

    /// <summary>
    /// Obtiene registros de auditoria criticos
    /// </summary>
    Task<IEnumerable<AuditLog>> GetCriticosAsync(DateTime? desde = null, DateTime? hasta = null);

    /// <summary>
    /// Busqueda general de registros de auditoria
    /// </summary>
    Task<IEnumerable<AuditLog>> BuscarAsync(
        string? entidad = null,
        string? usuario = null,
        string? modulo = null,
        string? accion = null,
        string? nivelCriticidad = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        int limite = 100);

    // ==================== CONSULTAS PAGINADAS ====================

    /// <summary>
    /// Busqueda paginada con filtros
    /// </summary>
    Task<(IEnumerable<AuditLog> Registros, int Total)> BuscarPaginadoAsync(AuditoriaFiltrosDto filtros);

    /// <summary>
    /// Obtiene un registro por ID
    /// </summary>
    Task<AuditLog?> GetByIdAsync(long id);

    // ==================== ESTADISTICAS ====================

    /// <summary>
    /// Cuenta registros por periodo
    /// </summary>
    Task<int> ContarRegistrosAsync(DateTime? desde = null, DateTime? hasta = null);

    /// <summary>
    /// Cuenta registros por nivel de criticidad
    /// </summary>
    Task<Dictionary<string, int>> ContarPorNivelCriticidadAsync(DateTime? desde = null, DateTime? hasta = null);

    /// <summary>
    /// Cuenta registros por modulo
    /// </summary>
    Task<Dictionary<string, int>> ContarPorModuloAsync(DateTime? desde = null, DateTime? hasta = null);

    /// <summary>
    /// Cuenta registros por accion
    /// </summary>
    Task<Dictionary<string, int>> ContarPorAccionAsync(DateTime? desde = null, DateTime? hasta = null);

    /// <summary>
    /// Obtiene usuarios mas activos
    /// </summary>
    Task<IEnumerable<(string Usuario, int Cantidad, DateTime? UltimaActividad)>> GetUsuariosMasActivosAsync(
        int top = 10, DateTime? desde = null, DateTime? hasta = null);

    /// <summary>
    /// Cuenta registros por dia
    /// </summary>
    Task<IEnumerable<(DateTime Fecha, int Cantidad, int Criticos)>> GetActividadPorDiaAsync(
        DateTime desde, DateTime hasta);

    /// <summary>
    /// Cuenta registros por hora del dia
    /// </summary>
    Task<Dictionary<int, int>> ContarPorHoraAsync(DateTime? desde = null, DateTime? hasta = null);

    /// <summary>
    /// Cuenta IPs unicas
    /// </summary>
    Task<int> ContarIpsUnicasAsync(DateTime? desde = null, DateTime? hasta = null);

    /// <summary>
    /// Cuenta usuarios unicos
    /// </summary>
    Task<int> ContarUsuariosUnicosAsync(DateTime? desde = null, DateTime? hasta = null);

    // ==================== RETENCION ====================

    /// <summary>
    /// Obtiene configuracion de retencion
    /// </summary>
    Task<ConfiguracionRetencionDto?> GetConfiguracionRetencionAsync();

    /// <summary>
    /// Actualiza configuracion de retencion
    /// </summary>
    Task ActualizarConfiguracionRetencionAsync(ConfiguracionRetencionDto config);

    /// <summary>
    /// Elimina registros antiguos segun politica de retencion
    /// </summary>
    Task<int> LimpiarRegistrosAntiguosAsync(int diasRetencion, string? nivelExcluir = null);

    /// <summary>
    /// Archiva registros antes de eliminar
    /// </summary>
    Task<int> ArchivarRegistrosAsync(DateTime hasta, string rutaArchivo);

    // ==================== ALERTAS ====================

    /// <summary>
    /// Cuenta intentos fallidos de login por usuario en periodo
    /// </summary>
    Task<int> ContarFallosLoginAsync(string? usuario, string? ip, int minutos);

    /// <summary>
    /// Detecta actividad inusual
    /// </summary>
    Task<IEnumerable<AuditLog>> GetActividadInusualAsync(string usuario, DateTime desde);
}
