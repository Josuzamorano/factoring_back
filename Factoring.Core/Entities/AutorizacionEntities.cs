namespace Factoring.Core.Entities;

/// <summary>
/// Modulo del sistema (agrupacion logica de funcionalidades)
/// </summary>
public class Modulo
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Accion disponible en un modulo (ver, crear, editar, eliminar, aprobar, etc.)
/// </summary>
public class Accion
{
    public int Id { get; set; }
    public int ModuloId { get; set; }
    public string Codigo { get; set; } = string.Empty; // ej: "operaciones.crear", "operaciones.aprobar"
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool RequiereAprobacion { get; set; }
    public decimal? MontoMaximo { get; set; } // Limite de monto para esta accion
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Rol del sistema (agrupacion de permisos)
/// </summary>
public class Rol
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Nivel { get; set; } // Nivel jerarquico (1=mas alto)
    public int? RolPadreId { get; set; } // Para herencia de permisos
    public bool EsAdministrador { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Relacion Rol-Accion (permisos asignados a un rol)
/// </summary>
public class RolPermiso
{
    public int Id { get; set; }
    public int RolId { get; set; }
    public int AccionId { get; set; }
    public bool Permitido { get; set; } = true;
    public decimal? MontoMaximo { get; set; } // Override del monto maximo de la accion
    public int? SucursalId { get; set; } // Restriccion por sucursal (null = todas)
}

/// <summary>
/// Relacion Usuario-Rol
/// </summary>
public class UsuarioRol
{
    public int Id { get; set; }
    public string UsuarioLogin { get; set; } = string.Empty;
    public int RolId { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Permisos especificos de un usuario (override de rol)
/// </summary>
public class UsuarioPermiso
{
    public int Id { get; set; }
    public string UsuarioLogin { get; set; } = string.Empty;
    public int AccionId { get; set; }
    public bool Permitido { get; set; }
    public decimal? MontoMaximo { get; set; }
    public int? SucursalId { get; set; }
    public string? Motivo { get; set; } // Razon del permiso especial
    public DateTime FechaAsignacion { get; set; }
    public string AsignadoPor { get; set; } = string.Empty;
}

/// <summary>
/// Restriccion por monto para operaciones
/// </summary>
public class RestriccionMonto
{
    public int Id { get; set; }
    public int RolId { get; set; }
    public string TipoOperacion { get; set; } = string.Empty; // Operacion, Pago, LineaCredito
    public decimal MontoMinimo { get; set; }
    public decimal MontoMaximo { get; set; }
    public bool RequiereAprobacion { get; set; }
    public int? RolAprobadorId { get; set; } // Rol que puede aprobar
}

/// <summary>
/// Solicitud de aprobacion (flujo 4 ojos)
/// </summary>
public class SolicitudAprobacion
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty; // Operacion, LineaCredito, Pago, etc.
    public int EntidadId { get; set; } // ID de la entidad a aprobar
    public string SolicitadoPor { get; set; } = string.Empty;
    public DateTime FechaSolicitud { get; set; }
    public string? AprobadoPor { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string Estado { get; set; } = "Pendiente"; // Pendiente, Aprobado, Rechazado, Cancelado
    public string? Comentarios { get; set; }
    public decimal? Monto { get; set; }
    public int NivelAprobacionRequerido { get; set; } // Nivel minimo del rol aprobador
}

/// <summary>
/// Historial de aprobaciones (para auditoría)
/// </summary>
public class HistorialAprobacion
{
    public int Id { get; set; }
    public int SolicitudId { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty; // Aprobar, Rechazar, Comentar
    public DateTime Fecha { get; set; }
    public string? Comentarios { get; set; }
}

/// <summary>
/// Delegacion temporal de permisos
/// </summary>
public class DelegacionPermiso
{
    public int Id { get; set; }
    public string UsuarioOrigen { get; set; } = string.Empty;
    public string UsuarioDestino { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string? Motivo { get; set; }
    public bool Activa { get; set; } = true;
}

/// <summary>
/// Vista consolidada de permisos efectivos de un usuario
/// </summary>
public class PermisoEfectivo
{
    public string UsuarioLogin { get; set; } = string.Empty;
    public string ModuloCodigo { get; set; } = string.Empty;
    public string AccionCodigo { get; set; } = string.Empty;
    public bool Permitido { get; set; }
    public decimal? MontoMaximo { get; set; }
    public int? SucursalId { get; set; }
    public string Origen { get; set; } = string.Empty; // Rol, Usuario, Delegacion
    public bool RequiereAprobacion { get; set; }
}
