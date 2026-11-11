namespace Factoring.Application.DTOs;

// =============================================
// DTOs para Operaciones - Versión simplificada
// =============================================

/// <summary>
/// DTO para crear una operación
/// </summary>
public class CreateOperacionDto
{
    public long RutCliente { get; set; }
    public int TipoDocumento { get; set; } = 1;
    public DateTime? FechaOperacion { get; set; } = DateTime.Today;
    public int TipoMoneda { get; set; } = 1;
    public decimal PorcentajeAnticipo { get; set; } = 80;
    public int? CodigoBanco { get; set; }
    public string? CuentaCorriente { get; set; }
    public string? Observaciones { get; set; }
    
    // Documentos a incluir
    public List<CreateDocumentoDto> Facturas { get; set; } = new();
    public List<CreateDocumentoDto> Cheques { get; set; } = new();
    public List<CreateDocumentoDto> Letras { get; set; } = new();
    public List<CreateDocumentoDto> Otros { get; set; } = new();
    public List<CreateGastoDto> Gastos { get; set; } = new();
}

/// <summary>
/// DTO para crear un documento
/// </summary>
public class CreateDocumentoDto
{
    public long RutDeudor { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public int? TipoDocumento { get; set; } // Solo para "Otros"
    public DateTime? FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public int TipoMoneda { get; set; } = 1;
    public int? CodigoBanco { get; set; } // Solo para cheques
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para crear un gasto
/// </summary>
public class CreateGastoDto
{
    public int CodigoGasto { get; set; }
    public decimal MontoGasto { get; set; }
    public bool AfectoIva { get; set; }
}

/// <summary>
/// DTO para actualizar una operación
/// </summary>
public class UpdateOperacionDto
{
    public long NumeroOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public DateTime? FechaOperacion { get; set; }
    public int TipoMoneda { get; set; }
    public decimal PorcentajeAnticipo { get; set; }
    public int? CodigoBanco { get; set; }
    public string? CuentaCorriente { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para respuesta de operación en lista
/// </summary>
public class OperacionListDto
{
    public long NumeroOperacion { get; set; }
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime? FechaOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public string NombreTipoDocumento { get; set; } = string.Empty;
    public int EstadoOperacion { get; set; }
    public string NombreEstado { get; set; } = string.Empty;
    public int TipoMoneda { get; set; }
    public decimal MontoDocumentos { get; set; }
    public decimal TotalGiro { get; set; }
}

/// <summary>
/// DTO para respuesta de operación completa
/// </summary>
public class OperacionDto
{
    public long NumeroOperacion { get; set; }
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public int TipoDocumento { get; set; }
    public string NombreTipoDocumento { get; set; } = string.Empty;
    public DateTime? FechaOperacion { get; set; }
    public DateTime? FechaOtorgamiento { get; set; }
    public int EstadoOperacion { get; set; }
    public string NombreEstado { get; set; } = string.Empty;
    public int TipoMoneda { get; set; }
    public string NombreMoneda { get; set; } = string.Empty;
    
    // Montos
    public decimal MontoDocumentos { get; set; }
    public decimal PorcentajeAnticipo { get; set; }
    public decimal MontoAnticipo { get; set; }
    public decimal MontoAdelanto { get; set; }
    public decimal MontoGastos { get; set; }
    public decimal MontoIvaComision { get; set; }
    public decimal TotalGiro { get; set; }
    
    // Banco
    public int? CodigoBanco { get; set; }
    public string? NombreBanco { get; set; }
    public string? CuentaCorriente { get; set; }
    
    public string? Observaciones { get; set; }
    public DateTime? FechaCreacion { get; set; }
    
    // Documentos
    public List<DocumentoDto> Facturas { get; set; } = new();
    public List<DocumentoDto> Cheques { get; set; } = new();
    public List<DocumentoDto> Letras { get; set; } = new();
    public List<DocumentoDto> Otros { get; set; } = new();
    public List<GastoDto> Gastos { get; set; } = new();
}

/// <summary>
/// DTO para documento
/// </summary>
public class DocumentoDto
{
    public long CorrelativoOperacion { get; set; }
    public int CorrelativoDocumento { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public DateTime? FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal MontoAbonado { get; set; }
    public decimal MontoSaldo { get; set; }
    public int EstadoDocumento { get; set; }
    public string NombreEstado { get; set; } = string.Empty;
    public int TipoMoneda { get; set; }
    public int? CodigoBanco { get; set; }
    public string? NombreBanco { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para gasto
/// </summary>
public class GastoDto
{
    public long CorrelativoOperacion { get; set; }
    public int CorrelativoGasto { get; set; }
    public int CodigoGasto { get; set; }
    public string DescripcionGasto { get; set; } = string.Empty;
    public decimal MontoGasto { get; set; }
    public bool AfectoIva { get; set; }
}

/// <summary>
/// DTO para búsqueda de operaciones
/// </summary>
public class OperacionSearchDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public long? RutCliente { get; set; }
    public int? Estado { get; set; }
    public int? TipoDocumento { get; set; }
}

/// <summary>
/// DTO para otorgar operación
/// </summary>
public class OtorgarOperacionDto
{
    public long NumeroOperacion { get; set; }
    public DateTime? FechaOtorgamiento { get; set; } = DateTime.Today;
}

/// <summary>
/// DTO para cambiar estado de operación
/// </summary>
public class CambiarEstadoDto
{
    public long NumeroOperacion { get; set; }
    public int NuevoEstado { get; set; }
}
