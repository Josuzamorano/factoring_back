// =============================================
// Entidades para Módulo 3 - Carpeta Deudor
// Migrado de: F_CD1100.frm, F_CD1200.frm
// =============================================

namespace Factoring.Core.Entities;

/// <summary>
/// Deudor - Migrado de tabla DEU
/// </summary>
public class Deudor
{
    public int RutCliente { get; set; }
    public int RutDeudor { get; set; }
    public string? DigitoVerificador { get; set; }
    public int? CodigoSucursal { get; set; }
    public string? RazonSocial { get; set; }
    public string? Direccion { get; set; }
    public int? CodigoComuna { get; set; }
    public int EstadoDeudor { get; set; } = 1;
    public string? ContactoCobranza { get; set; }
    public decimal PorcentajeVentas { get; set; }
    public int Notificacion { get; set; }
    public decimal MontoOtorgado { get; set; }
    public decimal MontoOcupado { get; set; }
    public int ChequesGirados { get; set; }
    public string? LugarPago { get; set; }
    public string? DireccionPago { get; set; }
    public int NumeroProtestos { get; set; }
    public string? ContactoCobranza1 { get; set; }
    public string? CargoNotificacion { get; set; }
    public int? ActividadEconomica { get; set; }
    public int? CategoriaRiesgo { get; set; }
    public string? AtencionNotificacion { get; set; }
    public string? GiroDeudor { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    // Propiedades calculadas
    public decimal MontoDisponible => MontoOtorgado - MontoOcupado;
    public bool Activo => EstadoDeudor == 1;
    public string RutFormateado => $"{RutDeudor:N0}-{DigitoVerificador}".Replace(",", ".");

    // Propiedades de navegación (para vistas)
    public string? NombreComuna { get; set; }
    public string? NombreCliente { get; set; }
    public string? DescripcionCategoriaRiesgo { get; set; }
}

/// <summary>
/// Teléfono/Contacto Deudor - Migrado de tabla TED
/// </summary>
public class DeudorTelefono
{
    public int RutCliente { get; set; }
    public int RutDeudor { get; set; }
    public int TipoComunicacion { get; set; }
    public string? Descripcion { get; set; }
    public string NumeroComunicacion { get; set; } = string.Empty;
    public int Estado { get; set; } = 1;
    public DateTime FechaCreacion { get; set; }

    // Descripción del tipo
    public string TipoComunicacionDescripcion => TipoComunicacion switch
    {
        1 => "Teléfono Fijo",
        2 => "Celular",
        3 => "Email",
        4 => "Fax",
        _ => "Otro"
    };
}

/// <summary>
/// Documento Deudor - Migrado de tabla DEU_DOC
/// </summary>
public class DeudorDocumento
{
    public int RutCliente { get; set; }
    public int RutDeudor { get; set; }
    public int NumeroDocumento { get; set; }
    public int TipoDocumento { get; set; }
    public string NombreDocumento { get; set; } = string.Empty;
    public string? RutaDocumento { get; set; }
    public DateTime? FechaDocumento { get; set; }
    public string? Observaciones { get; set; }
    public int Estado { get; set; } = 1;
    public DateTime FechaCreacion { get; set; }
}

/// <summary>
/// Grupo de Deudores
/// </summary>
public class GrupoDeudor
{
    public long RutGrupo { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public decimal MontoLineaAprobada { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioCreacion { get; set; }
    public string? UsuarioModificacion { get; set; }
    
    // Propiedades calculadas
    public int CantidadDeudores { get; set; }
    public decimal MontoOcupado { get; set; }
    public decimal MontoDisponible => MontoLineaAprobada - MontoOcupado;
}

/// <summary>
/// Relación Deudor-Grupo
/// </summary>
public class DeudorGrupo
{
    public int Id { get; set; }
    public long RutGrupo { get; set; }
    public long RutDeudor { get; set; }
    public string? NombreGrupo { get; set; }
    public string? NombreDeudor { get; set; }
    public string? NombreFantasia { get; set; }
    public string? Giro { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public string? UsuarioCreacion { get; set; }
}
