namespace Factoring.Core.Entities;

/// <summary>
/// Entidad para registro de auditoria de cambios criticos
/// Registra operaciones CRUD en entidades sensibles del sistema
/// </summary>
public class AuditLog
{
    public long AudId { get; set; }

    /// <summary>
    /// Tipo de entidad afectada (Operacion, Cliente, LineaCredito, etc.)
    /// </summary>
    public string AudEntidad { get; set; } = string.Empty;

    /// <summary>
    /// ID de la entidad afectada
    /// </summary>
    public string AudEntidadId { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de accion: INSERT, UPDATE, DELETE, STATUS_CHANGE
    /// </summary>
    public string AudAccion { get; set; } = string.Empty;

    /// <summary>
    /// Usuario que realizo la accion
    /// </summary>
    public string AudUsuario { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora de la accion
    /// </summary>
    public DateTime AudFechaHora { get; set; }

    /// <summary>
    /// Direccion IP del cliente
    /// </summary>
    public string? AudIp { get; set; }

    /// <summary>
    /// Nombre de la maquina cliente
    /// </summary>
    public string? AudMaquina { get; set; }

    /// <summary>
    /// Valores anteriores en formato JSON (para UPDATE y DELETE)
    /// </summary>
    public string? AudValoresAnteriores { get; set; }

    /// <summary>
    /// Valores nuevos en formato JSON (para INSERT y UPDATE)
    /// </summary>
    public string? AudValoresNuevos { get; set; }

    /// <summary>
    /// Descripcion adicional de la operacion
    /// </summary>
    public string? AudDescripcion { get; set; }

    /// <summary>
    /// ID de correlacion para trazabilidad
    /// </summary>
    public string? AudCorrelationId { get; set; }

    /// <summary>
    /// Modulo del sistema (Operaciones, Cobranza, Tesoreria, etc.)
    /// </summary>
    public string? AudModulo { get; set; }

    /// <summary>
    /// Nivel de criticidad: LOW, MEDIUM, HIGH, CRITICAL
    /// </summary>
    public string AudNivelCriticidad { get; set; } = "MEDIUM";
}

/// <summary>
/// Tipos de acciones de auditoria
/// </summary>
public static class AuditAccion
{
    public const string Insert = "INSERT";
    public const string Update = "UPDATE";
    public const string Delete = "DELETE";
    public const string View = "VIEW";
    public const string StatusChange = "STATUS_CHANGE";
    public const string Login = "LOGIN";
    public const string Logout = "LOGOUT";
    public const string LoginFailed = "LOGIN_FAILED";
    public const string PasswordChange = "PASSWORD_CHANGE";
    public const string Export = "EXPORT";
    public const string Import = "IMPORT";
    public const string Approve = "APPROVE";
    public const string Reject = "REJECT";
}

/// <summary>
/// Niveles de criticidad para auditoria
/// </summary>
public static class AuditNivel
{
    public const string Low = "LOW";
    public const string Medium = "MEDIUM";
    public const string High = "HIGH";
    public const string Critical = "CRITICAL";
}

/// <summary>
/// Modulos del sistema para auditoria
/// </summary>
public static class AuditModulo
{
    public const string Seguridad = "SEGURIDAD";
    public const string Operaciones = "OPERACIONES";
    public const string LineasCredito = "LINEAS_CREDITO";
    public const string Cobranza = "COBRANZA";
    public const string Tesoreria = "TESORERIA";
    public const string Clientes = "CLIENTES";
    public const string Legal = "LEGAL";
    public const string Informes = "INFORMES";
    public const string Configuracion = "CONFIGURACION";
}
