using Factoring.Core.Entities;

namespace Factoring.Core.Interfaces;

/// <summary>
/// Repositorio para gestion de autorizacion granular
/// </summary>
public interface IAutorizacionRepository
{
    // ==================== MODULOS ====================
    Task<IEnumerable<Modulo>> GetModulosAsync();
    Task<Modulo?> GetModuloByIdAsync(int id);
    Task<Modulo?> GetModuloByCodigoAsync(string codigo);

    // ==================== ACCIONES ====================
    Task<IEnumerable<Accion>> GetAccionesAsync();
    Task<IEnumerable<Accion>> GetAccionesByModuloAsync(int moduloId);
    Task<Accion?> GetAccionByIdAsync(int id);
    Task<Accion?> GetAccionByCodigoAsync(string codigo);

    // ==================== ROLES ====================
    Task<IEnumerable<Rol>> GetRolesAsync();
    Task<Rol?> GetRolByIdAsync(int id);
    Task<Rol?> GetRolByCodigoAsync(string codigo);
    Task<int> CrearRolAsync(Rol rol);
    Task ActualizarRolAsync(Rol rol);
    Task EliminarRolAsync(int rolId);
    Task<int> ContarUsuariosPorRolAsync(int rolId);

    // ==================== PERMISOS DE ROL ====================
    Task<IEnumerable<RolPermiso>> GetPermisosRolAsync(int rolId);
    Task AsignarPermisoRolAsync(RolPermiso permiso);
    Task RevocarPermisoRolAsync(int rolId, int accionId);
    Task ActualizarPermisosRolAsync(int rolId, IEnumerable<RolPermiso> permisos);

    // ==================== USUARIOS Y ROLES ====================
    Task<IEnumerable<UsuarioRol>> GetRolesUsuarioAsync(string usuarioLogin);
    Task AsignarRolUsuarioAsync(UsuarioRol asignacion);
    Task RevocarRolUsuarioAsync(string usuarioLogin, int rolId);
    Task<bool> UsuarioTieneRolAsync(string usuarioLogin, int rolId);

    // ==================== PERMISOS ESPECIALES DE USUARIO ====================
    Task<IEnumerable<UsuarioPermiso>> GetPermisosUsuarioAsync(string usuarioLogin);
    Task AsignarPermisoUsuarioAsync(UsuarioPermiso permiso);
    Task RevocarPermisoUsuarioAsync(int permisoId);

    // ==================== PERMISOS EFECTIVOS ====================
    Task<IEnumerable<PermisoEfectivo>> GetPermisosEfectivosAsync(string usuarioLogin);
    Task<PermisoEfectivo?> GetPermisoEfectivoAsync(string usuarioLogin, string accionCodigo);
    Task<bool> TienePermisoAsync(string usuarioLogin, string accionCodigo, decimal? monto = null, int? sucursalId = null);

    // ==================== RESTRICCIONES POR MONTO ====================
    Task<IEnumerable<RestriccionMonto>> GetRestriccionesMontoAsync();
    Task<IEnumerable<RestriccionMonto>> GetRestriccionesMontoRolAsync(int rolId);
    Task<RestriccionMonto?> GetRestriccionMontoAsync(int rolId, string tipoOperacion);
    Task<int> CrearRestriccionMontoAsync(RestriccionMonto restriccion);
    Task ActualizarRestriccionMontoAsync(RestriccionMonto restriccion);
    Task EliminarRestriccionMontoAsync(int restriccionId);

    // ==================== APROBACIONES ====================
    Task<IEnumerable<SolicitudAprobacion>> GetSolicitudesPendientesAsync();
    Task<IEnumerable<SolicitudAprobacion>> GetSolicitudesPendientesParaUsuarioAsync(string usuarioLogin);
    Task<IEnumerable<SolicitudAprobacion>> GetSolicitudesPorEntidadAsync(string tipo, int entidadId);
    Task<SolicitudAprobacion?> GetSolicitudAprobacionAsync(int solicitudId);
    Task<int> CrearSolicitudAprobacionAsync(SolicitudAprobacion solicitud);
    Task ActualizarSolicitudAprobacionAsync(SolicitudAprobacion solicitud);
    Task<IEnumerable<HistorialAprobacion>> GetHistorialAprobacionAsync(int solicitudId);
    Task RegistrarHistorialAprobacionAsync(HistorialAprobacion historial);

    // ==================== DELEGACIONES ====================
    Task<IEnumerable<DelegacionPermiso>> GetDelegacionesActivasAsync(string usuarioLogin);
    Task<IEnumerable<DelegacionPermiso>> GetDelegacionesOtorgadasAsync(string usuarioLogin);
    Task<int> CrearDelegacionAsync(DelegacionPermiso delegacion);
    Task CancelarDelegacionAsync(int delegacionId);
    Task<bool> TieneDelegacionActivaAsync(string usuarioOrigen, string usuarioDestino);

    // ==================== ESTADISTICAS ====================
    Task<int> ContarSolicitudesPendientesAsync();
    Task<int> ContarDelegacionesActivasAsync();
}
