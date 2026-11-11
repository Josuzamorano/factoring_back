namespace Factoring.Domain.Entities;

/// <summary>
/// Operación de Factoring - Tabla OPE
/// </summary>
public class Operacion
{
    public long NumeroOperacion { get; set; }
    public long RutCliente { get; set; }
    public int TipoDocumento { get; set; }
    public DateTime? FechaOperacion { get; set; }
    public int EstadoOperacion { get; set; }
    public int TipoMoneda { get; set; }
    public decimal MontoDocumentos { get; set; }
    public DateTime? FechaOtorgamiento { get; set; }
    public decimal PorcentajeAnticipo { get; set; }
    public decimal MontoAnticipo { get; set; }
    public decimal MontoAdelanto { get; set; }
    public decimal MontoGastos { get; set; }
    public decimal MontoIvaComision { get; set; }
    public decimal TotalGiro { get; set; }
    public int? CodigoBanco { get; set; }
    public string? CuentaCorriente { get; set; }
    public string? Observaciones { get; set; }
    public string? UsuarioCreacion { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public string? UsuarioModificacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    
    // Campos para visualización (no mapeados a BD)
    public string? NombreCliente { get; set; }
    public string? NombreTipoDocumento { get; set; }
    public string? NombreEstado { get; set; }
    public string? NombreMoneda { get; set; }
    public string? NombreBanco { get; set; }
}

/// <summary>
/// Factura Otorgada - Tabla FAO
/// </summary>
public class FacturaOtorgada
{
    public long CorrelativoOperacion { get; set; }
    public int CorrelativoDocumento { get; set; }
    public long RutDeudor { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime? FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal MontoAbonado { get; set; }
    public decimal MontoSaldo { get; set; }
    public int EstadoDocumento { get; set; }
    public int TipoMoneda { get; set; }
    public string? Observaciones { get; set; }
    public DateTime? FechaCreacion { get; set; }
    
    // Campos para visualización
    public string? NombreDeudor { get; set; }
    public string? NombreEstado { get; set; }
}

/// <summary>
/// Cheque Otorgado - Tabla CHO
/// </summary>
public class ChequeOtorgado
{
    public long CorrelativoOperacion { get; set; }
    public int CorrelativoDocumento { get; set; }
    public long RutDeudor { get; set; }
    public string NumeroCheque { get; set; } = string.Empty;
    public int? CodigoBanco { get; set; }
    public DateTime? FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal MontoAbonado { get; set; }
    public decimal MontoSaldo { get; set; }
    public int EstadoDocumento { get; set; }
    public int TipoMoneda { get; set; }
    public string? Observaciones { get; set; }
    public DateTime? FechaCreacion { get; set; }
    
    // Campos para visualización
    public string? NombreDeudor { get; set; }
    public string? NombreBanco { get; set; }
    public string? NombreEstado { get; set; }
}

/// <summary>
/// Letra Otorgada - Tabla LEO
/// </summary>
public class LetraOtorgada
{
    public long CorrelativoOperacion { get; set; }
    public int CorrelativoDocumento { get; set; }
    public long RutDeudor { get; set; }
    public string NumeroLetra { get; set; } = string.Empty;
    public DateTime? FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal MontoAbonado { get; set; }
    public decimal MontoSaldo { get; set; }
    public int EstadoDocumento { get; set; }
    public int TipoMoneda { get; set; }
    public string? Observaciones { get; set; }
    public DateTime? FechaCreacion { get; set; }
    
    // Campos para visualización
    public string? NombreDeudor { get; set; }
    public string? NombreEstado { get; set; }
}

/// <summary>
/// Otro Documento Otorgado - Tabla OTO
/// </summary>
public class OtroDocumentoOtorgado
{
    public long CorrelativoOperacion { get; set; }
    public int CorrelativoDocumento { get; set; }
    public long RutDeudor { get; set; }
    public int TipoDocumento { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public DateTime? FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal MontoAbonado { get; set; }
    public decimal MontoSaldo { get; set; }
    public int EstadoDocumento { get; set; }
    public int TipoMoneda { get; set; }
    public string? Observaciones { get; set; }
    public DateTime? FechaCreacion { get; set; }
    
    // Campos para visualización
    public string? NombreDeudor { get; set; }
    public string? NombreTipoDocumento { get; set; }
    public string? NombreEstado { get; set; }
}

/// <summary>
/// Documento de Operación - Vista unificada VW_DOCUMENTOS_OPERACION
/// </summary>
public class DocumentoOperacion
{
    public long CorrelativoOperacion { get; set; }
    public int CorrelativoDocumento { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public DateTime? FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal MontoAbonado { get; set; }
    public decimal MontoSaldo { get; set; }
    public int EstadoDocumento { get; set; }
    public int TipoMoneda { get; set; }
    public string? Observaciones { get; set; }
    
    // Campos para visualización
    public string? NombreDeudor { get; set; }
    public string? NombreEstado { get; set; }
}

/// <summary>
/// Tipo de Gasto - Tabla GAS
/// </summary>
public class TipoGasto
{
    public int CodigoGasto { get; set; }
    public string DescripcionGasto { get; set; } = string.Empty;
    public decimal PorcentajeGasto { get; set; }
    public decimal MontoGasto { get; set; }
    public bool AfectoIva { get; set; }
    public int EstadoGasto { get; set; }
    public DateTime? FechaCreacion { get; set; }
}

/// <summary>
/// Gasto de Operación - Tabla GDS
/// </summary>
public class GastoOperacion
{
    public long CorrelativoOperacion { get; set; }
    public int CorrelativoGasto { get; set; }
    public int CodigoGasto { get; set; }
    public decimal MontoGasto { get; set; }
    public bool AfectoIva { get; set; }
    public DateTime? FechaCreacion { get; set; }
    
    // Campos para visualización
    public string? DescripcionGasto { get; set; }
}
