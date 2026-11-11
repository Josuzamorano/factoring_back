namespace Factoring.Core.DTOs;

// ==================== MODULOS Y ACCIONES ====================

public class ModuloDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }
    public int Orden { get; set; }
    public List<AccionDto> Acciones { get; set; } = new();
}

public class AccionDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool RequiereAprobacion { get; set; }
    public decimal? MontoMaximo { get; set; }
}

// ==================== ROLES ====================

public class RolDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Nivel { get; set; }
    public int? RolPadreId { get; set; }
    public string? RolPadreNombre { get; set; }
    public bool EsAdministrador { get; set; }
    public int CantidadUsuarios { get; set; }
    public List<PermisoRolDto> Permisos { get; set; } = new();
}

public class PermisoRolDto
{
    public int AccionId { get; set; }
    public string AccionCodigo { get; set; } = string.Empty;
    public string AccionNombre { get; set; } = string.Empty;
    public string ModuloCodigo { get; set; } = string.Empty;
    public string ModuloNombre { get; set; } = string.Empty;
    public bool Permitido { get; set; }
    public decimal? MontoMaximo { get; set; }
    public int? SucursalId { get; set; }
    public string? SucursalNombre { get; set; }
}

public class CrearRolRequest
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Nivel { get; set; } = 10;
    public int? RolPadreId { get; set; }
    public bool EsAdministrador { get; set; }
    public List<AsignarPermisoRequest> Permisos { get; set; } = new();
}

public class AsignarPermisoRequest
{
    public int AccionId { get; set; }
    public bool Permitido { get; set; } = true;
    public decimal? MontoMaximo { get; set; }
    public int? SucursalId { get; set; }
}

// ==================== USUARIOS Y PERMISOS ====================

public class UsuarioPermisosDto
{
    public string UsuarioLogin { get; set; } = string.Empty;
    public string UsuarioNombre { get; set; } = string.Empty;
    public List<RolAsignadoDto> Roles { get; set; } = new();
    public List<PermisoEspecialDto> PermisosEspeciales { get; set; } = new();
    public List<PermisoEfectivoDto> PermisosEfectivos { get; set; } = new();
}

public class RolAsignadoDto
{
    public int RolId { get; set; }
    public string RolCodigo { get; set; } = string.Empty;
    public string RolNombre { get; set; } = string.Empty;
    public DateTime FechaAsignacion { get; set; }
    public DateTime? FechaExpiracion { get; set; }
}

public class PermisoEspecialDto
{
    public int Id { get; set; }
    public string AccionCodigo { get; set; } = string.Empty;
    public string AccionNombre { get; set; } = string.Empty;
    public bool Permitido { get; set; }
    public decimal? MontoMaximo { get; set; }
    public string? Motivo { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public string AsignadoPor { get; set; } = string.Empty;
}

public class PermisoEfectivoDto
{
    public string ModuloCodigo { get; set; } = string.Empty;
    public string ModuloNombre { get; set; } = string.Empty;
    public string AccionCodigo { get; set; } = string.Empty;
    public string AccionNombre { get; set; } = string.Empty;
    public bool Permitido { get; set; }
    public decimal? MontoMaximo { get; set; }
    public bool RequiereAprobacion { get; set; }
    public string Origen { get; set; } = string.Empty;
}

public class AsignarRolRequest
{
    public string UsuarioLogin { get; set; } = string.Empty;
    public int RolId { get; set; }
    public DateTime? FechaExpiracion { get; set; }
}

public class AsignarPermisoUsuarioRequest
{
    public string UsuarioLogin { get; set; } = string.Empty;
    public int AccionId { get; set; }
    public bool Permitido { get; set; }
    public decimal? MontoMaximo { get; set; }
    public int? SucursalId { get; set; }
    public string? Motivo { get; set; }
}

// ==================== RESTRICCIONES POR MONTO ====================

public class RestriccionMontoDto
{
    public int Id { get; set; }
    public int RolId { get; set; }
    public string RolNombre { get; set; } = string.Empty;
    public string TipoOperacion { get; set; } = string.Empty;
    public decimal MontoMinimo { get; set; }
    public decimal MontoMaximo { get; set; }
    public bool RequiereAprobacion { get; set; }
    public int? RolAprobadorId { get; set; }
    public string? RolAprobadorNombre { get; set; }
}

public class CrearRestriccionMontoRequest
{
    public int RolId { get; set; }
    public string TipoOperacion { get; set; } = string.Empty;
    public decimal MontoMinimo { get; set; }
    public decimal MontoMaximo { get; set; }
    public bool RequiereAprobacion { get; set; }
    public int? RolAprobadorId { get; set; }
}

// ==================== APROBACIONES (4 OJOS) ====================

public class SolicitudAprobacionDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int EntidadId { get; set; }
    public string? EntidadDescripcion { get; set; }
    public string SolicitadoPor { get; set; } = string.Empty;
    public string SolicitadoPorNombre { get; set; } = string.Empty;
    public DateTime FechaSolicitud { get; set; }
    public string? AprobadoPor { get; set; }
    public string? AprobadoPorNombre { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Comentarios { get; set; }
    public decimal? Monto { get; set; }
    public int NivelAprobacionRequerido { get; set; }
    public bool PuedeAprobar { get; set; } // Para el usuario actual
}

public class CrearSolicitudAprobacionRequest
{
    public string Tipo { get; set; } = string.Empty;
    public int EntidadId { get; set; }
    public string? Comentarios { get; set; }
    public decimal? Monto { get; set; }
}

public class ProcesarAprobacionRequest
{
    public int SolicitudId { get; set; }
    public bool Aprobar { get; set; }
    public string? Comentarios { get; set; }
}

public class HistorialAprobacionDto
{
    public string Usuario { get; set; } = string.Empty;
    public string UsuarioNombre { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string? Comentarios { get; set; }
}

// ==================== DELEGACION ====================

public class DelegacionDto
{
    public int Id { get; set; }
    public string UsuarioOrigen { get; set; } = string.Empty;
    public string UsuarioOrigenNombre { get; set; } = string.Empty;
    public string UsuarioDestino { get; set; } = string.Empty;
    public string UsuarioDestinoNombre { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string? Motivo { get; set; }
    public bool Activa { get; set; }
}

public class CrearDelegacionRequest
{
    public string UsuarioDestino { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string? Motivo { get; set; }
}

// ==================== VERIFICACION ====================

public class VerificarPermisoRequest
{
    public string AccionCodigo { get; set; } = string.Empty;
    public decimal? Monto { get; set; }
    public int? SucursalId { get; set; }
}

public class VerificarPermisoResponse
{
    public bool Permitido { get; set; }
    public bool RequiereAprobacion { get; set; }
    public string? MotivoRechazo { get; set; }
    public decimal? MontoMaximoPermitido { get; set; }
    public int? NivelAprobacionRequerido { get; set; }
}

// ==================== RESUMEN SEGURIDAD ====================

public class ResumenSeguridadDto
{
    public int TotalUsuarios { get; set; }
    public int TotalRoles { get; set; }
    public int TotalModulos { get; set; }
    public int TotalAcciones { get; set; }
    public int SolicitudesPendientes { get; set; }
    public int DelegacionesActivas { get; set; }
    public List<RolResumenDto> RolesMasUsados { get; set; } = new();
}

public class RolResumenDto
{
    public string RolNombre { get; set; } = string.Empty;
    public int CantidadUsuarios { get; set; }
}
